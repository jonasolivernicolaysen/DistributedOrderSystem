namespace InventoryService.Models
{
    public class PaymentCompletedItem
    {
        public Guid ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
