using PaymentService.Models.DTOs;
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
        public DateTime? PaidAt { get; set; } 
        public int ReceivingAccount { get; set; } = 1234567890; // account number of product owner, user Id of product owner etc
        public string UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public List<PaymentItem> Items { get; set; }
    }
}