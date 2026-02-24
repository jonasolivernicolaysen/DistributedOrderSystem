namespace InventoryService.Models.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public Guid MessageId { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
