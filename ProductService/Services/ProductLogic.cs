using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.Exceptions;

namespace ProductService.Services
{
    public class ProductLogic
    {
        private readonly ProductDbContext _context;

        public ProductLogic(ProductDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            var products = await _context.Products.ToListAsync();
            return products;
        }

        public async Task<Product> GetProductByIdAsync(Guid Id)
        {
            var product = await _context.Products.FindAsync(Id);
            if (product == null)
                throw new NotFoundException(Id.ToString());
            return product;            
        }
    }
}
