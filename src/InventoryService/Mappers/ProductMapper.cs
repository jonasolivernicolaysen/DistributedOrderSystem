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
                Description = productCreatedEvent.Description,
                Price = productCreatedEvent.Price,
                Stock = 0
            };
        }
        public static InventoryModel ProductUpdatedEventToInventoryModel(ProductUpdatedEvent productUpdatedEvent)
        {
            return new InventoryModel
            {
                Id = Guid.NewGuid(),
                ProductId = productUpdatedEvent.ProductId,
                ProductName = productUpdatedEvent.ProductName,
                Description = productUpdatedEvent.Description,
                Price = productUpdatedEvent.Price,
                Stock = 0
            };
        }
    }
}
