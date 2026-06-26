using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using System.Text.Json;
using OrderService.Models;
using SharedContracts;
using Microsoft.Extensions.Logging;

namespace OrderService.Messaging
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMQPublisher _publisher;
        private readonly ILogger<OutboxProcessor> _logger;  

        public OutboxProcessor(
            IServiceScopeFactory scopeFactory,
            RabbitMQPublisher publisher,
            ILogger<OutboxProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _publisher = publisher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                var messages = await db.OutboxMessages
                    .Where(m => !m.Processed)
                    .ToListAsync();

                foreach (var msg in messages)
                {
                    try
                    {
                        // using switch in case outboxprocessor should take multiple types in the future
                        switch (msg.Type)
                        {
                            case nameof(OrderCreatedEvent):
                                var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(msg.Payload);
                                if (orderCreatedEvent != null)
                                {
                                    _publisher.Publish(orderCreatedEvent, "orders");
                                    msg.Processed = true;
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"Outbox message: {ex.Message}");
                    }
                }
                await db.SaveChangesAsync();
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
