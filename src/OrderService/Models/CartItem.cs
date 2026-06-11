using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class CartItem
    {
        [Key]
        public Guid CartItemId { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public Guid CartId { get; set; }
        public Cart Cart { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}