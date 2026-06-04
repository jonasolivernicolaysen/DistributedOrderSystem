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


            double price = (double)product.price;
            Console.WriteLine($"product id: {product.productId}");
            Console.WriteLine($"Price: {product.price}");
            Console.WriteLine(response);
            Console.WriteLine(content);

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
    }
}
