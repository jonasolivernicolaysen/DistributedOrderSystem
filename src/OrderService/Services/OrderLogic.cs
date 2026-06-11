using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Exceptions;
using OrderService.Mappers;
using OrderService.Models;
using OrderService.Models.DTOs;
using SharedContracts;
using System.Text;
using System.Text.Json;

namespace OrderService.Services
{
    public class OrderLogic
    {
        private readonly OrderDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public OrderLogic(
            OrderDbContext context, 
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        /*
        public async Task<OrderModel> ProcessOrderCreation(CreateOrderDto dto, string userId)
        {
            // call product service to get product details and calculate total price
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7165/api/products/{dto.ProductId}");

            var token = _httpContextAccessor
                .HttpContext?
                .Request
                .Headers["Authorization"]
                .ToString();

            // forward jwt token
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductResponseDto>(content);
            if (product == null)
                throw new NotFoundException("Product not found");

            decimal price = (decimal)product.price;

            var order = OrderMapper.ToOrderModel(dto, userId);
            order.UnitPrice = price;
            _context.Models.Add(order);

            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = order.OrderId,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                PaymentId = order.PaymentId,
                UserId = order.UserId,
                UnitPrice = price
            };

            _context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = nameof(OrderCreatedEvent),
                Payload = JsonSerializer.Serialize(orderCreatedEvent),
                CreatedAt = DateTime.UtcNow,
                Processed = false
            });
            await _context.SaveChangesAsync();
            return order;
        }
        */

        public async Task<CartItem> AddItemToCartAsync(AddToCartDto dto, string userId)
        {
            // find cart in db
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = userId
                };

                _context.Carts.Add(cart);
            }

            // get stock from InventoryService
            var inventoryRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7248/api/inventory/{dto.ProductId}");

            var token = _httpContextAccessor
                .HttpContext?
                .Request
                .Headers["Authorization"]
                .ToString();

            // forward jwt token
            inventoryRequest.Headers.Add("Authorization", token);

            var inventoryResponse = await _httpClient.SendAsync(inventoryRequest);
            if (!inventoryResponse.IsSuccessStatusCode)
            {
                throw new NotFoundException("Product not found");
            }

            var inventoryContent = await inventoryResponse.Content.ReadAsStringAsync();
            var inventoryItem = JsonSerializer.Deserialize<InventoryResponseDto>(inventoryContent);

            if (inventoryItem == null)
                throw new NotFoundException("Inventory item not found");

            // find total quantity of product in cart
            var totalQuantityInCart = cart.Items
                .Where(i => i.ProductId == dto.ProductId)
                .Sum(i => i.Quantity + dto.Quantity);


            // order quantity cannot exceed stock
            if (inventoryItem.stock < totalQuantityInCart)
                throw new BadRequestException($"Insufficient stock. Stock: {inventoryItem.stock}");


            // get product by productId from ProductService
            var productRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7165/api/products/{dto.ProductId}");

            // forward jwt token
            productRequest.Headers.Add("Authorization", token);

            var productResponse = await _httpClient.SendAsync(productRequest);
            if (!productResponse.IsSuccessStatusCode)
            {
                throw new NotFoundException("Product not found");
            }

            var productContent = await productResponse.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductResponseDto>(productContent);
            if (product == null)
                throw new NotFoundException("Product not found");

            decimal price = (decimal)product.price;

            // check if same product is already in cart. If it is, add it to quantity.
            var existingItem = cart.Items
                .FirstOrDefault(i => i.ProductId == dto.ProductId);

            CartItem cartItem;
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                cartItem = existingItem;
            } else
            {
                // refactor to CartItem
                cartItem = new CartItem
                {
                    ProductId = dto.ProductId,
                    UnitPrice = price,
                    Quantity = dto.Quantity,
                    CartId = cart.CartId,
                    Name = product.name,
                    Description = product.description
                };

                // add item to cart
                _context.CartItems.Add(cartItem);
            }
            // save
            try
            {
                var changes = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            // return cartitem
            return cartItem;
        }

        public async Task<Cart> GetCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
                throw new NotFoundException("Cart not found");
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<OrderModel> CheckoutAsync(string userId)
        {
            try
            {

                var cart = await GetCartAsync(userId);
                if (!cart.Items.Any())
                    throw new BadRequestException("Cart is empty");

                var order = new OrderModel
                {
                    UserId = userId,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    TotalPrice = cart.Items.Sum(i => i.UnitPrice * i.Quantity)
                };

                order.Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList();

                _context.Models.Add(order);

                // add order to outboxmessages

                var orderCreatedEvent = new OrderCreatedEvent
                {
                    OrderId = order.OrderId,
                    PaymentId = order.PaymentId,
                    UserId = order.UserId,
                    TotalPrice = order.TotalPrice,
                    Items = order.Items.Select(i => new OrderCreatedItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                };

                _context.OutboxMessages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = nameof(OrderCreatedEvent),
                    Payload = JsonSerializer.Serialize(orderCreatedEvent),
                    CreatedAt = DateTime.UtcNow,
                    Processed = false
                });

                cart.Items.Clear();

                await _context.SaveChangesAsync();

                return order;
            } catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }
    }
}
