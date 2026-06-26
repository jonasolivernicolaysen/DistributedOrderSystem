using PaymentService.Data;
using PaymentService.Messaging;
using PaymentService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedContracts;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using OrderService.Messaging;
using Microsoft.Extensions.Logging;

namespace PaymentService.Messaging
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
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

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
                            case nameof(PaymentCompletedEvent):
                                var paymentCompletedEvent = JsonSerializer.Deserialize<PaymentCompletedEvent>(msg.Payload);
                                if (paymentCompletedEvent != null)
                                {
                                    _publisher.Publish(paymentCompletedEvent, "payments");
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
