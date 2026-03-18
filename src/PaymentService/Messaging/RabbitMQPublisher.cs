using Microsoft.EntityFrameworkCore.Metadata;
using OrderService.Messaging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PaymentService.Messaging
{
    public class RabbitMQPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;

        public RabbitMQPublisher()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Publish<T>(T message, string exchangeName)
        {
            _channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Fanout,
                durable: true);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: exchangeName,
                routingKey: "",
                basicProperties: null,
                body: body);
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Dispose();
        }
    }
}
