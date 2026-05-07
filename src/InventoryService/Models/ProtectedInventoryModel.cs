namespace InventoryService.Models
{
    public class ProtectedInventoryModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Stock { get; set; }
    }
}
