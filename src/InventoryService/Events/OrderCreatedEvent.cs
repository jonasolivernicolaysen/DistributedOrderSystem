namespace InventoryService.Events
{
    public class OrderCreatedEvent
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public int Quantity { get; set; }
    }
}
