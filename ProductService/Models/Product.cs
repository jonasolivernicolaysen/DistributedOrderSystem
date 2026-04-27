using System.ComponentModel.DataAnnotations;

namespace ProductService.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        [Range(0.01, 1000000)]
        public decimal Price { get; set; }
    }
}
