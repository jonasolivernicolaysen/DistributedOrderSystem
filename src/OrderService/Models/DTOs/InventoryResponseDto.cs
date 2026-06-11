namespace OrderService.Models.DTOs
{
    public class InventoryResponseDto
    {
        public Guid productId { get; set; }
        public string productName { get; set; }
        public string? description { get; set; }
        public decimal price { get; set; }
        public int stock { get; set; }
    }
}
