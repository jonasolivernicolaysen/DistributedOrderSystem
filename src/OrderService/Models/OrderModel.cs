using RabbitMQ.Client;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class OrderModel
    {
        [Key]
        public Guid OrderId { get; set; } = Guid.NewGuid();
        public Guid PaymentId { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalPrice { get; set; } 
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
