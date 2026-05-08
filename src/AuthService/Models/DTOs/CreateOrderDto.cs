using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.DTOs
{
    public class CreateOrderDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
