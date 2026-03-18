using OrderService.Data;
using OrderService.Mappers;
using OrderService.Models;
using OrderService.Models.DTOs;
using SharedContracts;
using System.Text.Json;

namespace OrderService.Services
{
    public class OrderLogic
    {
        private readonly OrderDbContext _context;
        public OrderLogic(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<OrderModel> ProcessOrderCreation(CreateOrderDto dto)
        {
            var order = OrderMapper.ToOrderModel(dto);
            _context.Models.Add(order);

            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = order.OrderId,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                PaymentId = order.PaymentId
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
