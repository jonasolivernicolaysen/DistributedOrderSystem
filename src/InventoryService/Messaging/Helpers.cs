
using RabbitMQ.Client.Events;

namespace InventoryService.Messaging
{
    public class Helpers
    {
        public int GetRetryCount2(BasicDeliverEventArgs ea)
        {
            if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-death", out var deathHeader))
            {
                var deaths = (List<object>)deathHeader;

                return deaths
                    .Select(d => (Dictionary<string, object>)d)
                    .Sum(d => Convert.ToInt32(d["count"]));
            }
            return 0;
        }
    }
}
