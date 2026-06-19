using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models
{
    public class PaymentItem
    {
        [Key]
        public Guid PaymentItemId { get; set; } = Guid.NewGuid();
        public Guid PaymentId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
