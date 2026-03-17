using System.ComponentModel.DataAnnotations;

namespace PaymentService.Models
{
    public class OutBoxMessage
    {
        [Key]
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Processed { get; set; }
    }
}
