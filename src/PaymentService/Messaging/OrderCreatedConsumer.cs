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

                    var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(json);
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
            }
        }
    }
}
