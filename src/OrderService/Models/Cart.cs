namespace OrderService.Models
{
    public class Cart
    {
        public Guid CartId { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
