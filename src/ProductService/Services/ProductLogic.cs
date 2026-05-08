using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Exceptions;
using ProductService.Models;
using ProductService.Models.DTOs;
using SharedContracts;
using System.Text.Json;
using System.Security.Claims;

namespace ProductService.Services
{
    public class ProductLogic
    {
        private readonly ProductDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductLogic(
            ProductDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
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

            var currentUserId = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (currentUserId == null)
                throw new UnauthorizedException("User must be authenticated to create a product listing");

            var product = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                OwnerId = currentUserId
            };
            _context.Products.Add(product);

            // add to outboxmessages 
            var evt = new ProductCreatedEvent
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Description = product.Description,
                Price = dto.Price
            };

            _context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = nameof(ProductCreatedEvent),
                Payload = JsonSerializer.Serialize(evt),
                CreatedAt = DateTime.UtcNow,
                Processed = false
            });

            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProductListingAsync(Guid productId, UpdateProductListingDto dto)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new NotFoundException($"Product with id {productId} was not found");

            // check if updated product name is taken

            var updatedNameIsTaken = await _context.Products.AnyAsync(p => p.Name == dto.Name && p.ProductId != productId);
            if (updatedNameIsTaken)
                throw new BadRequestException($"Product with name {dto.Name} already exists");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;

            _context.Products.Update(product);

            // add to outboxmessages 
            var evt = new ProductUpdatedEvent
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Description = product.Description,
                Price = product.Price
            };

            _context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = nameof(ProductUpdatedEvent),
                Payload = JsonSerializer.Serialize(evt),
                CreatedAt = DateTime.UtcNow,
                Processed = false
            });

            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> DeleteProductListingAsync(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new NotFoundException($"Product with id {productId} was not found");

            // add to outboxmessages 
            var evt = new ProductDeletedEvent
            {
                ProductId = product.ProductId
            };

            _context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = nameof(ProductDeletedEvent),
                Payload = JsonSerializer.Serialize(evt),
                CreatedAt = DateTime.UtcNow,
                Processed = false
            });

            await _context.SaveChangesAsync();
            return product;
        }
    }
}
