namespace OrderService.Models.DTOs
{
    public class CartResponseDto
    {
        public Guid CartId { get; set; }
        public List<CartItemResponseDto> Items { get; set; } = new List<CartItemResponseDto>();
    }
}
