using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models.DTOs
{
    public class PaymentDto
    {
        public Guid PaymentId { get; set; }
        public int PayingAccount { get; set; }
    }
}
