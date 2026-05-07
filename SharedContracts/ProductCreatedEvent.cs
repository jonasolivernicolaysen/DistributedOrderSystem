using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedContracts
{
    public class ProductCreatedEvent
    {
        [Required]
        public Guid ProductId { get; set; }
        [Required]
        public string ProductName { get; set; }
        public Guid MessageId { get; set; } = Guid.NewGuid();
    }
}
