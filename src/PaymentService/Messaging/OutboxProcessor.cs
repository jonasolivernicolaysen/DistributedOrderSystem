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

namespace PaymentService.Messaging
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly PaymentPublisher _publisher;

        public OutboxProcessor(
            IServiceScopeFactory scopeFactory,
            PaymentPublisher publisher)
        {
            _scopeFactory = scopeFactory;
            _publisher = publisher; 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

                var messages = await db.OutBoxMessages
                    .Where(m => !m.Processed)
                    .ToListAsync();

                foreach (var msg in messages)
                {
                    try
                    {
                        var evt = JsonSerializer.Deserialize<PaymentCompletedEvent>(msg.Payload);
                        if (evt != null)
                        {
                            _publisher.Publish(evt);
                            msg.Processed = true;
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
