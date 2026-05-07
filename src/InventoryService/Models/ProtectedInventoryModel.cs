using System.ComponentModel.DataAnnotations;

namespace InventoryService.Models
{
    public class ProtectedInventoryModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string? Description { get; set; }
        [Range(0.01, 1000000)]
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
