using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedContracts
{
    public class ProductDeletedEvent
    {
        public Guid ProductId { get; set; }
        public Guid MessageId { get; set; } = Guid.NewGuid();
    }
}
