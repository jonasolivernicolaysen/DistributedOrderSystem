using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class OrderModel
    {
        [Key]
        public Guid OrderId { get; set; } = Guid.NewGuid();
        public Guid PaymentId { get; set; } = Guid.NewGuid();
        [Required]
        public Guid ProductId { get; set; }
        public OrderStatus Status { get; set; }
        [Required, Range(1, 100000)]
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
