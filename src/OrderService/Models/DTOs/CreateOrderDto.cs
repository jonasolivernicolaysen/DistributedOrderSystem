using System.ComponentModel.DataAnnotations;

namespace OrderService.Models.DTOs
{
    public class CreateOrderDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
