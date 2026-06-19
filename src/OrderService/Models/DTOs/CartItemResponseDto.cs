namespace OrderService.Models.DTOs
{
    public class CartItemResponseDto
    {
        public Guid ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
