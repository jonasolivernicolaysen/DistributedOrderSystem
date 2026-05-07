using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.Exceptions;
using ProductService.Models.DTOs;

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

        public async Task<Product> GetProductByIdAsync(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new NotFoundException(productId.ToString());
            return product;            
        }

        public async Task<Product> CreateProductListingAsync(CreateProductListingDto dto)
        {
            // check if product name is taken
            var nameIsTaken = await _context.Products.AnyAsync(p => p.Name == dto.Name);
            if (nameIsTaken)
                throw new BadRequestException($"Product with name {dto.Name} already exists");

            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProductListingAsync(Guid productId, UpdateProductListingDto dto)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new NotFoundException($"Product with id {productId} was not found");

            // check if updated product name is taken
            var updatedNameIsTaken = await _context.Products.AnyAsync(p => p.Name == dto.Name);
            if (updatedNameIsTaken)
                throw new BadRequestException($"Product with name {dto.Name} already exists");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }
    }
}
