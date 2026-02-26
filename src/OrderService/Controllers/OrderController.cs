using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using OrderService.Messaging;
using OrderService.Models.DTOs;
using OrderService.Mappers;
using SharedContracts;
using System.Text.Json.Serialization;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly OrderPublisher _publisher;
        public OrderController(
            OrderDbContext context,
            OrderPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            var order = OrderMapper.ToOrderModel(dto);
            _context.Add(order);

            await _context.SaveChangesAsync();

            // publish event
            var evt = new OrderCreatedEvent
            {
                OrderId = order.OrderId,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                PaymentId = order.PaymentId
            };

            _publisher.Publish(evt);

            return Ok(new CreateOrderResponseDto { 
                PaymentId = order.PaymentId
            });
        } 
    }
}
