using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models
{
    public class PaymentModel
    {
        [Key]
        public Guid PaymentId { get; set; }
        [Required]
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime? PaidAt { get; set; } 
        public int ReceivingAccount { get; set; } = 1234567890; // account number of website owner
        public int PayingAccount { get; set; }
    }
}

