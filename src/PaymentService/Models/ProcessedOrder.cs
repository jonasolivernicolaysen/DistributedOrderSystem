namespace PaymentService.Models
{
    public class ProcessedOrder
    {
        public int Id { get; set; }
        public Guid MessageId { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
