using InventoryService.Data;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedContracts;
using System.Text.Json;
using System.Text;
using InventoryService.Mappers;
using InventoryService.Exceptions;
using Microsoft.Extensions.Logging;

namespace InventoryService.Messaging
{
    public class ProductEventsConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private RabbitMQ.Client.IModel _channel;
        private ILogger<ProductEventsConsumer> _logger;

        private const string QueueName = "inventory-products";
        private const string ExchangeName = "products";

        public ProductEventsConsumer(
            IServiceScopeFactory scopeFactory,
            ILogger<ProductEventsConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // declare main queue and queue and bind exchange to queue
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);

            var mainQueueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "inventory-products-retry" },
                { "x-dead-letter-routing-key", "retry"}
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

            // declare retry exchange and queue and bind exchange to queue
            _channel.ExchangeDeclare("inventory-products-retry", ExchangeType.Direct, durable: true);

            var retryArgs = new Dictionary<string, object>
            {
                {"x-message-ttl", 10000 },
                {"x-dead-letter-exchange", "products"}
            };

            _channel.QueueDeclare(
                "inventory-products-retry-10s",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: retryArgs);

            _channel.QueueBind(
                queue: "inventory-products-retry-10s",
                exchange: "inventory-products-retry",
                routingKey: "retry");

            // declare dlx and dlq
            _channel.ExchangeDeclare("inventory-products-dlx", ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(
                "inventory-products-dlq",
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                queue: "inventory-products-dlq",
                exchange: "inventory-products-dlx",
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
                    var eventType = ea.BasicProperties.Type;

                    switch (eventType)
                    {
                        case nameof(ProductCreatedEvent):
                            {
                                var evt = JsonSerializer.Deserialize<ProductCreatedEvent>(json);

                                _logger.LogInformation($"Received MessageId: {evt?.MessageId} at {DateTime.UtcNow}");
                                if (evt == null)
                                {
                                    _channel.BasicReject(ea.DeliveryTag, requeue: false);
                                    return;
                                }
                                await HandleProductCreatedEvent(evt);
                                break;
                            }
                            
                        case nameof(ProductUpdatedEvent):
                            {
                                var evt = JsonSerializer.Deserialize<ProductUpdatedEvent>(json);

                                _logger.LogInformation($"Received MessageId: {evt?.MessageId} at {DateTime.UtcNow}");
                                if (evt == null)
                                {
                                    _channel.BasicReject(ea.DeliveryTag, requeue: false);
                                    return;
                                }
                                await HandleProductUpdatedEvent(evt);
                                break;
                            }
                        case nameof(ProductDeletedEvent):
                            {
                                var evt = JsonSerializer.Deserialize<ProductDeletedEvent>(json);

                                _logger.LogInformation($"Received MessageId: {evt?.MessageId} at {DateTime.UtcNow}");
                                if (evt == null)
                                {
                                    _channel.BasicReject(ea.DeliveryTag, requeue: false);
                                    return;
                                }
                                await HandleProductDeletedEvent(evt);
                                break;
                            }
                        default:
                            throw new BadRequestException($"Unknown event type: {eventType}");
                    }
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());

                    var retryCount = GetRetryCount(ea);

                    if (retryCount > 1)
                    {
                        _channel.BasicPublish(
                            "inventory-products-dlx",
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

        private async Task HandleProductCreatedEvent(ProductCreatedEvent evt)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var alreadyProcessed = await db.ProcessedMessages
                .AnyAsync(message => message.MessageId == evt.MessageId);

            if (alreadyProcessed)
                return;

            var existingProduct = await db.Inventory
                .AnyAsync(p => p.ProductId == evt.ProductId);

            if (existingProduct)
                return;

            var product = ProductMapper.ProductCreatedEventToInventoryModel(evt);

            var processedMessage = new ProcessedMessage
            {
                MessageId = evt.MessageId,
                ProcessedAt = DateTime.UtcNow
            };

            // add product to db
            db.Inventory.Add(product);
            db.ProcessedMessages.Add(processedMessage);
            await db.SaveChangesAsync();
        }

        private async Task HandleProductUpdatedEvent(ProductUpdatedEvent evt)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var alreadyProcessed = await db.ProcessedMessages
                .AnyAsync(message => message.MessageId == evt.MessageId);

            if (alreadyProcessed)
                return;

            var existingProduct = await db.Inventory
                .FirstOrDefaultAsync(p => p.ProductId == evt.ProductId);

            if (existingProduct == null)
                return;

            existingProduct.ProductName = evt.ProductName;
            existingProduct.Description = evt.Description;
            existingProduct.Price = evt.Price;

            var processedMessage = new ProcessedMessage
            {
                MessageId = evt.MessageId,
                ProcessedAt = DateTime.UtcNow
            };

            db.ProcessedMessages.Add(processedMessage);
            await db.SaveChangesAsync();
        }

        private async Task HandleProductDeletedEvent(ProductDeletedEvent evt)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

            var alreadyProcessed = await db.ProcessedMessages
                .AnyAsync(message => message.MessageId == evt.MessageId);

            if (alreadyProcessed)
                return;

            var existingProduct = await db.Inventory
                .FirstOrDefaultAsync(p => p.ProductId == evt.ProductId);

            if (existingProduct == null)
                return;

            // removes product from db
            db.Remove(existingProduct);

            var processedMessage = new ProcessedMessage
            {
                MessageId = evt.MessageId,
                ProcessedAt = DateTime.UtcNow
            };

            db.ProcessedMessages.Add(processedMessage);
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
