namespace InventoryService.Models
{
    public class InventoryModel
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Stock { get; set; }
    }
}
