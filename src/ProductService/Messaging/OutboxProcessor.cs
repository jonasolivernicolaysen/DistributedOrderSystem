using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using SharedContracts;
using System.Text.Json;

namespace ProductService.Messaging
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
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

                    var messages = await db.OutboxMessages
                        .Where(m => !m.Processed)
                        .OrderBy(m => m.CreatedAt)
                        .ToListAsync(stoppingToken);

                    foreach (var msg in messages)
                    {
                        try
                        {
                            switch (msg.Type)
                            {
                                case nameof(ProductCreatedEvent):
                                    {
                                        var evt = JsonSerializer.Deserialize<ProductCreatedEvent>(msg.Payload);

                                        if (evt != null)
                                        {
                                            _publisher.Publish(evt, "products");
                                            msg.Processed = true;
                                        }

                                        break;
                                    }

                                case nameof(ProductUpdatedEvent):
                                    {
                                        var evt = JsonSerializer.Deserialize<ProductUpdatedEvent>(msg.Payload);

                                        if (evt != null)
                                        {
                                            _publisher.Publish(evt, "products");
                                            msg.Processed = true;
                                        }

                                        break;
                                    }

                                case nameof(ProductDeletedEvent):
                                    {
                                        var evt = JsonSerializer.Deserialize<ProductDeletedEvent>(msg.Payload);

                                        if (evt != null)
                                        {
                                            _publisher.Publish(evt, "products");
                                            msg.Processed = true;
                                        }

                                        break;
                                    }

                                default:
                                    _logger.LogWarning("Unknown outbox message type: {Type}", msg.Type);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Failed processing outbox message {Id}", msg.Id);
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "OutboxProcessor failed.");
                }

                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}