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
    public class OrderCreatedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IModel _channel;

        private const string ExchangeName = "orders";
        private const string QueueName = "payment-service";

        public OrderCreatedConsumer(IServiceScopeFactory scopeFactory)
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
                {"x-dead-letter-exchange", "payments-retry" },
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
            _channel.ExchangeDeclare("payments-retry", ExchangeType.Direct, durable: true);

            var retryArgs = new Dictionary<string, object>
            {
                {"x-message-ttl", 10000 },
                {"x-dead-letter-exchange", "orders"}
            };

            _channel.QueueDeclare(
                "payments-retry-10s",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: retryArgs);

            _channel.QueueBind(
                queue: "payments-retry-10s",
                exchange: "payments-retry",
                routingKey: "retry");

            // declare dlx and dlq
            _channel.ExchangeDeclare("payments-dlx", ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(
                "payment-service-dlq",
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                queue: "payment-service-dlq",
                exchange: "payments-dlx",
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

                    var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(json);
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
                    Console.WriteLine(ex.Message);

                    var retryCount = GetRetryCount(ea);

                    if (retryCount > 1)
                    {
                        _channel.BasicPublish(
                            "payments-dlx",
                            "",
                            ea.BasicProperties,
                            ea.Body);

                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                    else
                    {
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

        private async Task HandleEvent(OrderCreatedEvent evt)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

            var alreadyProcessed = await db.ProcessedOrders
                .AnyAsync(message => message.MessageId == evt.MessageId);

            if (alreadyProcessed)
            {
                Console.WriteLine("Order already processed");
                return;
            }

            var payment = new PaymentModel
            {
                PaymentId = evt.PaymentId,
                OrderId = evt.OrderId,
                ProductId = evt.ProductId,
                Quantity = evt.Quantity,
                Status = PaymentStatus.Pending
            };

            var processedOrder = new ProcessedOrder
            {
                MessageId = evt.MessageId,
                ProcessedAt = DateTime.UtcNow
            };

            try
            {
                db.Payments.Add(payment);
                db.ProcessedOrders.Add(processedOrder);

                await db.SaveChangesAsync();
            } 
                catch (DbUpdateException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
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
