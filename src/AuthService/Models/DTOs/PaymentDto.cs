using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.DTOs
{
    public class PaymentDto
    {
        public Guid PaymentId { get; set; }
        public int PayingAccount { get; set; }
    }
}
