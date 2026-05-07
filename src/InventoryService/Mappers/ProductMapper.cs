using InventoryService.Models;
using SharedContracts;

namespace InventoryService.Mappers
{
    public class ProductMapper
    {
        public static InventoryModel ProductCreatedEventToInventoryModel(ProductCreatedEvent productCreatedEvent)
        {
            return new InventoryModel
            {
                Id = Guid.NewGuid(),
                ProductId = productCreatedEvent.ProductId,
                ProductName = productCreatedEvent.ProductName,
                Stock = 0
            };
        }
    }
}
