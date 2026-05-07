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

namespace ProductService.Messaging
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMQPublisher _publisher;

        public OutboxProcessor(
            IServiceScopeFactory scopeFactory,
            RabbitMQPublisher publisher)
        {
            _scopeFactory = scopeFactory;
            _publisher = publisher;
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
                        // using switch in case outboxprocessor should take multiple types in the future
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
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Outbox message: {ex.Message}");
                    }
                }
                await db.SaveChangesAsync();
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
