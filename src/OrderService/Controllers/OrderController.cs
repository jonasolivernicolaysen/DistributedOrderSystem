using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using OrderService.Mappers;
using OrderService.Messaging;
using OrderService.Models;
using OrderService.Models.DTOs;
using OrderService.Services;
using SharedContracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("orders")]
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
            var order = await _orderLogic.ProcessOrderCreation(dto);

            return Ok(new CreateOrderResponseDto { 
                PaymentId = order.PaymentId
            });
        } 
    }
}
