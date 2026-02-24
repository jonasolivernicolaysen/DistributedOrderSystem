using InventoryService.Data;
using InventoryService.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using InventoryService.Models;
using InventoryService.Messaging;
using InventoryService.Models.DTOs;

namespace InventoryService.Messaging
{
    public class OrderCreatedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IModel _channel;

        private const string ExchangeName = "orders";
        private const string QueueName = "inventory-service";

        public OrderCreatedConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // ensure exachange exists
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);

            // create queue
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // bind queue to exchange
            _channel.QueueBind(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: "");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(json);
                if (evt != null)
                {

                    await HandleEvent(evt);
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: true,
                consumer: consumer);

            return Task.CompletedTask;
        }

        private async Task HandleEvent(OrderCreatedEvent evt)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var alreadyProcessed = await db.ProcessedMessages
                .AnyAsync(message => message.MessageId == evt.MessageId);

            if (alreadyProcessed)
            {
                // message has already been processed
                Console.WriteLine("duplicate message ignored");
                return;
            }

            var item = await db.Inventory
                .FirstOrDefaultAsync(item => item.ProductId == evt.ProductId);

            if (item == null)
            {
                item = new InventoryModel
                {
                    ProductId = evt.ProductId,
                    Stock = 100
                };

                db.Inventory.Add(item);
            }
            item.Stock -= evt.Quantity;

            db.ProcessedMessages.Add(new MessageDto
            {
                MessageId = evt.MessageId,
                ProcessedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}
