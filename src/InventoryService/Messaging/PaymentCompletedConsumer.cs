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
using System.Runtime.InteropServices;

namespace InventoryService.Messaging
{
    public class PaymentCompletedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IModel _channel;

        private const string QueueName = "inventory-service";
        private const string ExchangeName = "payments";

        public PaymentCompletedConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // declare main exchange and queue and bind queue to exchange
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);

            var mainQueueArgs = new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", "inventory-retry" },
                {"x-dead-letter-routing-key", "retry" }
            };

            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: mainQueueArgs);

            _channel.QueueBind(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: "");


            // declare retry exchange and queue and bind queue to exchange
            _channel.ExchangeDeclare("inventory-retry", ExchangeType.Direct, durable: true);

            var retryArgs = new Dictionary<string, object>
            {
                {"x-message-ttl", 10000 },
                {"x-dead-letter-exchange", "payments"}
            };

            _channel.QueueDeclare(
                "inventory-retry-10s",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: retryArgs);

            _channel.QueueBind(
                queue: "inventory-retry-10s",
                exchange: "inventory-retry",
                routingKey: "retry");

            // declare dlx and dlq
            _channel.ExchangeDeclare("inventory-dlx", ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(
                "inventory-service-dlq",
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                queue: "inventory-service-dlq",
                exchange: "inventory-dlx",
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

                    Console.WriteLine($"Received MessageId: {evt?.MessageId} at {DateTime.UtcNow}");
                    if (evt == null)
                    {
                        _channel.BasicReject(ea.DeliveryTag, requeue: false);
                        return;
                    }
                    await HandleEvent(evt);
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }   
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    var retryCount = GetRetryCount(ea);

                    if (retryCount > 1)
                    {
                        _channel.BasicPublish(
                            "inventory-dlx",
                            "",
                            ea.BasicProperties,
                            ea.Body);

                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    } 
                    else
                    {
                        // send it to the retry queue
                        _channel.BasicReject(ea.DeliveryTag, requeue: false);
                    }
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
                    // stock == 100 for simplicity, this is outside the scope of this project
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

        private int GetRetryCount(BasicDeliverEventArgs ea) 
        {
            if (ea.BasicProperties.Headers != null &&
                ea.BasicProperties.Headers.TryGetValue("x-death", out var deathHeader))
            {
                var deaths = (List<object>)deathHeader;

                return deaths
                    .Select(d => (Dictionary<string, object>)d)
                    .Sum(d => Convert.ToInt32(d["count"]));
            }
            return 0;
        }
    }
}
