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

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var order = await _orderLogic.ProcessOrderCreation(dto, userId);

            return Ok(new CreateOrderResponseDto { 
                PaymentId = order.PaymentId
            });
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
                    UnitPrice = i.UnitPrice
                }).ToList()
            });
        }
    }
}
