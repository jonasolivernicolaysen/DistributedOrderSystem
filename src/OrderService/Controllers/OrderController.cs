using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using OrderService.Mappers;
using OrderService.Messaging;
using OrderService.Models;
using OrderService.Models.DTOs;
using OrderService.Services;
using SharedContracts;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly RabbitMQPublisher _publisher;
        private readonly OrderLogic _orderLogic;
        public OrderController(
            OrderDbContext context,
            RabbitMQPublisher publisher,
            OrderLogic orderLogic)
        {
            _context = context;
            _publisher = publisher;
            _orderLogic = orderLogic;
        }


        [HttpPost("cart")]
        public async Task<IActionResult> AddItemToCart(AddToCartDto dto)
        {
            // find cart
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _orderLogic.AddItemToCartAsync(dto, userId);
            return Ok();
        }

        [HttpGet("cart")]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Cart cart = await _orderLogic.GetCartAsync(userId);
            return Ok(new CartResponseDto
            {
                CartId = cart.CartId,
                Items = cart.Items.Select(i => new CartItemResponseDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Name = i.Name,
                    Description = i.Description
                }).ToList()
            });
        }

        [HttpDelete("cart/{productId}")]
        public async Task<IActionResult> DeleteItemFromCart(Guid productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Cart cart = await _orderLogic.DeleteItemFromCartAsync(userId, productId);

            return NoContent();
        }

        [HttpPost("cart/checkout")]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var rawOrder = await _orderLogic.CheckoutAsync(userId);
            
            var order = new OrderResponseDto
            {
                OrderId = rawOrder.OrderId,
                PaymentId = rawOrder.PaymentId,
                TotalPrice = rawOrder.TotalPrice,
                Items = rawOrder.Items.Select(i => new CartItemResponseDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            return Ok(order);
        }
    }
}
