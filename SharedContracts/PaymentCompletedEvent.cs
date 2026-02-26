using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedContracts
{
    public class PaymentCompletedEvent
    {
        public Guid OrderId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid ProductId { get; set; } 
        public int Quantity { get; set; }
        public Guid MessageId { get; set; } = Guid.NewGuid();
    }
}
