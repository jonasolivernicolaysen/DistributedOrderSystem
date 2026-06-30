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
        public string UserId { get; set; }
        public List<PaymentCompletedItem> Items { get; set; }
        public decimal TotalPrice { get; set; }
        public Guid MessageId { get; set; } = Guid.NewGuid();
    }
}
