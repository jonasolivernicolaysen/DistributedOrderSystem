using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using ProductService.Messaging;
using ProductService.Data;
using ProductService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedContracts;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ProductService.Messaging
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMQPublisher _publisher;
        private ILogger<OutboxProcessor> _logger;

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
                var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

                var messages = await db.OutboxMessages
                    .Where(m => !m.Processed)
                    .ToListAsync();

                foreach (var msg in messages)
                {
                    try
                    {
                        // using switch so outboxprocessor can handle multiple types 
                        switch (msg.Type)
                        {
                            case nameof(ProductCreatedEvent):
                                var productCreatedEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(msg.Payload);
                                if (productCreatedEvent != null)
                                {
                                    _publisher.Publish(productCreatedEvent, "products");
                                    msg.Processed = true;
                                }
                                break;

                            case nameof(ProductUpdatedEvent):
                                var productUpdatedEvent = JsonSerializer.Deserialize<ProductUpdatedEvent>(msg.Payload);
                                if (productUpdatedEvent != null)
                                {
                                    _publisher.Publish(productUpdatedEvent, "products");
                                    msg.Processed = true;
                                }
                                break;

                            case nameof(ProductDeletedEvent):
                                var productDeletedEvent = JsonSerializer.Deserialize<ProductDeletedEvent>(msg.Payload);
                                if (productDeletedEvent != null)
                                {
                                    _publisher.Publish(productDeletedEvent, "products");
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
