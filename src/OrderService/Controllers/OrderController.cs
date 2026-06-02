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
    }
}
