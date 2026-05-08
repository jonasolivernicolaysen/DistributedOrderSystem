using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.DTOs
{
    public class CreateProductListingDto
    {
        [Required]
        public required string Name { get; set; }

        public string? Description { get; set; }

        [Range(0.01, 1000000)]
        public decimal Price { get; set; }
    }
}
