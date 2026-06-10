namespace OrderService.Models.DTOs
{
    public class OrderResponseDto
    {
        public Guid OrderId { get; set; } = Guid.NewGuid();
        public Guid PaymentId { get; set; } = Guid.NewGuid();
        public decimal TotalPrice { get; set; }
        public List<CartItemResponseDto> Items { get; set; } = new List<CartItemResponseDto>();
    }
}
