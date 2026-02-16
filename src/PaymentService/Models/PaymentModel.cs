using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models
{
    public class PaymentModel
    {
        [Key]
        public Guid PaymentId { get; set; }
        [Required]
        public Guid OrderId { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime PaidAt { get; set; }
        public int ReceivingAccount { get; set; }
        public int PayingAccount { get; set; }
    }
}

