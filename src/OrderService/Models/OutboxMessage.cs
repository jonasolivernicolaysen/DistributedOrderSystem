using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class OutboxMessage
    {
        [Key]
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Processed { get; set; }
    }
}
