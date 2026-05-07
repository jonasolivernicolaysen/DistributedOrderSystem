using InventoryService.Models;

namespace InventoryService.Mappers
{
    public class InventoryMapper
    {
        public static ProtectedInventoryModel ToProtectedInventoryModel(InventoryModel model)
        {
            return new ProtectedInventoryModel
            {
                ProductId = model.ProductId,
                ProductName = model.ProductName,
                Stock = model.Stock
            };
        }
    }
}
