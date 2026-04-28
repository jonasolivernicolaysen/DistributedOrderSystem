using System.ComponentModel.DataAnnotations;

namespace ProductService.Models
{
    public class Product
    {
        public Guid ProductId { get; set; }
        [Required]
        public required string Name { get; set; }
        public string? Description { get; set; }

        [Range(0.01, 1000000)]
        public decimal Price { get; set; }
    }
}
