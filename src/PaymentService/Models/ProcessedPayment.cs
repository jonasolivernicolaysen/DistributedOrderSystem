using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models
{
    public class ProcessedPayment
    {
        [Key]
        public int Id { get; set; }
        public Guid MessageId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
