using ProductService.Models;
using ProductService.Models.DTOs;

namespace ProductService.Mappers
{
    public static class ProductMapper
    {
        public static Product ToModel(CreateProductListingDto dto)
        {
            return new Product
            {
                ProductId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price
            };
        }

        public static CreateProductListingDto ToDto(Product model)
        {
            return new CreateProductListingDto
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price
            };
        }
    }
}
