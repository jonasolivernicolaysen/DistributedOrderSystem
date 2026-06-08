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
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}