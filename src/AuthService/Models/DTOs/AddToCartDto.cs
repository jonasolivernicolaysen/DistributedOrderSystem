namespace AuthService.Models.DTOs
{
    public class AddToCartDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
