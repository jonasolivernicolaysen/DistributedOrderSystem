using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ProductService.Messaging
{
    public class RabbitMQPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQPublisher(
            IConfiguration configuration,
            ILogger<RabbitMQPublisher> logger)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"],
                Port = int.Parse(configuration["RabbitMQ:Port"]!),
                UserName = configuration["RabbitMQ:UserName"],
                Password = configuration["RabbitMQ:Password"]
            };

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
                        "RabbitMQ unavailable. Retrying in 3 seconds...");

                    Thread.Sleep(3000);
                }
            }
        }

        public void Publish<T>(T message, string exchangeName)
        {
            _channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Fanout,
                durable: true);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Type = typeof(T).Name;

            _channel.BasicPublish(
                exchange: exchangeName,
                routingKey: "",
                basicProperties: properties,
                body: body);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();

            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}