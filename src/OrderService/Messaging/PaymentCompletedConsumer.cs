using OrderService.Data;
using OrderService.Exceptions;
using OrderService.Messaging;
using OrderService.Services;
using OrderService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedContracts;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;


namespace OrderService.Messaging
{
    public class PaymentCompletedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IModel _channel;
        private readonly ILogger<PaymentCompletedConsumer> _logger;

        private const string QueueName = "orders-payments";
        private const string ExchangeName = "payments";

        public PaymentCompletedConsumer(
            IServiceScopeFactory scopeFactory,
            ILogger<PaymentCompletedConsumer> logger,
            IConfiguration configuration
            )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"],
                Port = int.Parse(configuration["RabbitMQ:Port"]!),
                UserName = configuration["RabbitMQ:UserName"],
                Password = configuration["RabbitMQ:Password"]
            };

            // retry logic which keeps service from crashing when running docker
            while (true)
            {
                try
                {
                    logger.LogInformation("Connecting to RabbitMQ...");

                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    logger.LogInformation("Connected to RabbitMQ.");

                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "RabbitMQ unavailable. Retrying in 5 seconds...");

                    Thread.Sleep(5000);
                }
            }

            // declare main exchange and queue and bind queue to exchange
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);

            var mainQueueArgs = new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", "orders-payments-retry" },
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
            _channel.ExchangeDeclare("orders-payments-retry", ExchangeType.Direct, durable: true);

            var retryArgs = new Dictionary<string, object>
            {
                {"x-message-ttl", 10000 },
                {"x-dead-letter-exchange", "payments"}
            };

            _channel.QueueDeclare(
                "orders-payments-retry-10s",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: retryArgs);

            _channel.QueueBind(
                queue: "orders-payments-retry-10s",
                exchange: "orders-payments-retry",
                routingKey: "retry");

            // declare dlx and dlq
            _channel.ExchangeDeclare("orders-payments-dlx", ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(
                "orders-payments-dlq",
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                queue: "orders-payments-dlq",
                exchange: "orders-payments-dlx",
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

                    _logger.LogInformation($"Received MessageId: {evt?.MessageId} at {DateTime.UtcNow}");
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
                    _logger.LogError(ex.ToString());

                    var retryCount = GetRetryCount(ea);

                    if (retryCount > 1)
                    {
                        _channel.BasicPublish(
                            "orders-payments-dlx",
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

            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var orderLogic = scope.ServiceProvider.GetRequiredService<OrderLogic>();

            var alreadyProcessed = await db.ProcessedMessages
                .AnyAsync(message => message.MessageId == evt.MessageId);

            if (alreadyProcessed)
            {
                // message has already been processed
                _logger.LogInformation("message has already been processed");
                return;
            }

            var cart = await orderLogic.GetCartAsync(evt.UserId);
            if (!cart.Items.Any())
                throw new BadRequestException("Cart is empty");

            // empty cart
            cart.Items.Clear();

            db.ProcessedMessages.Add(new ProcessedMessage
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

