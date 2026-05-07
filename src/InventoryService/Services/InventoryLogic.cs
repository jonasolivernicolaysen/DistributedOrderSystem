using InventoryService.Data;
using InventoryService.Exceptions;
using InventoryService.Mappers;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Services
{
    public class InventoryLogic
    {
        private readonly InventoryDbContext _context;

        public InventoryLogic(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryModel> UpdateProductStockAsync(Guid productId, int updatedStock)
        {
            var product = await _context.Inventory.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
                throw new NotFoundException($"Product with id {productId} not found");

            if (updatedStock <= 0)
                throw new BadRequestException("Stock quantity must be larger than 0");

            product.Stock = updatedStock;
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<List<InventoryModel>> GetProductsAsync()
        {
            var products = await _context.Inventory.ToListAsync();
            return products;
        }
    }
}
