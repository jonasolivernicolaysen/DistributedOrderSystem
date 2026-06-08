namespace OrderService.Models.DTOs
{
    public class CartItemResponseDto
    {
        public Guid ProductId { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
