using InventoryService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using InventoryService.Models;
using InventoryService.Messaging;
using SharedContracts;

namespace InventoryService.Messaging
{
    public class PaymentCompletedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IModel _channel;

        private const string ExchangeName = "payments";
        private const string QueueName = "inventory-service";

        public PaymentCompletedConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // ensure exchange exists
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
                try
                {

                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    var evt = JsonSerializer.Deserialize<PaymentCompletedEvent>(json);
                    if (evt == null)
                    {
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                        return;
                    }
                    await HandleEvent(evt);
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }   
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);

            return Task.CompletedTask;
        }

        private async Task HandleEvent(PaymentCompletedEvent evt)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var alreadyProcessed = await db.ProcessedMessages
                .AnyAsync(message => message.MessageId == evt.MessageId);

            if (alreadyProcessed)
            {
                // message has already been processed
                Console.WriteLine("message has already been processed");
                return;
            }

            // update inventory
            var item = await db.Inventory
                .FirstOrDefaultAsync(item => item.ProductId == evt.ProductId);

            if (item == null)
            {
                item = new InventoryModel
                {
                    ProductId = evt.ProductId,
                    // stock == 100 for simplicity, this is outside scope of this project
                    Stock = 100
                };

                db.Inventory.Add(item);
            }

            if (item.Stock < evt.Quantity)
            {
                Console.WriteLine("Not enough stock");
                return;
            }
            item.Stock -= evt.Quantity;

            db.ProcessedMessages.Add(new ProcessedPayment
            {
                MessageId = evt.MessageId,
                ProcessedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}
