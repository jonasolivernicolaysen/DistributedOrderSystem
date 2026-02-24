using System.ComponentModel.DataAnnotations;

namespace OrderService.Models.DTOs
{
    public class MessageDto
    {
        [Key]
        public int Id { get; set; }
        public Guid MessageId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
