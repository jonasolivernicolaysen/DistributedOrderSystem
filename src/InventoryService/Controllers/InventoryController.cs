using InventoryService.Mappers;
using InventoryService.Models.DTOs;
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
        public async Task<IActionResult> UpdateStock([FromRoute] Guid productId, [FromBody] UpdateStockDto dto)
        {
            var product = await _inventoryLogic.UpdateProductStockAsync(productId, dto.UpdatedStock);
            return Ok(InventoryMapper.ToProtectedInventoryModel(product));
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var inventory = await _inventoryLogic.GetProductsAsync();
            return Ok(inventory);
        }
    }
}
