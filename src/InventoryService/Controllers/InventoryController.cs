using InventoryService.Mappers;
using InventoryService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryLogic _inventoryLogic;

        public InventoryController(InventoryLogic inventoryLogic)
        {
            _inventoryLogic = inventoryLogic;
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateStock(Guid productId, int updatedStock)
        {
            var product = await _inventoryLogic.UpdateProductStockAsync(productId, updatedStock);
            return Ok(InventoryMapper.ToProtectedInventoryModel(product));
        }
    }
}
