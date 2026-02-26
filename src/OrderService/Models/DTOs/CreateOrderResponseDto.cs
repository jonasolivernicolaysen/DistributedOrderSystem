using RabbitMQ.Client;
using System.Text.Json.Serialization;

namespace OrderService.Models.DTOs
{
    public class CreateOrderResponseDto
    {
        [JsonPropertyName("payment-id")]
        public Guid PaymentId { get; set; }
    }
}
