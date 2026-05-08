using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedContracts
{
    public class ProductUpdatedEvent
    {
        [Required]
        public Guid ProductId { get; set; }
        [Required]
        public string ProductName { get; set; }
        public string? Description { get; set; }
        [Range(0.01, 1000000)]
        public decimal Price { get; set; }
        public Guid MessageId { get; set; } = Guid.NewGuid();
    }
}
