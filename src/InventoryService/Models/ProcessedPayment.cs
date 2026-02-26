namespace InventoryService.Models
{
    public class ProcessedPayment
    {
        public int Id { get; set; }
        public Guid MessageId { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}