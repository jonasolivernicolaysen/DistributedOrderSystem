using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class OrderModel
    {
        [Key]
        public Guid OrderId { get; set; }
        public Guid PaymentId { get; set; }
        public int UserId { get; set; }
        [Required]
        public Guid ProductId { get; set; }
        public OrderStatus Status { get; set; }
        [Range(1, 100000)]
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
