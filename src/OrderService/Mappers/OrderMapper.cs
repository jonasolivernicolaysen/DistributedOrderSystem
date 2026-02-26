using OrderService.Models;
using OrderService.Models.DTOs;

namespace OrderService.Mappers
{
    public static class OrderMapper
    {
        public static OrderModel ToOrderModel(CreateOrderDto dto)
        {
            return new OrderModel
            {
                ProductId = dto.ProductId,
                Status = OrderStatus.Pending,
                Quantity = dto.Quantity,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static CreateOrderDto ToDto(OrderModel model)
        {
            return new CreateOrderDto
            {
                ProductId = model.ProductId,
                Quantity = model.Quantity
            };
        }
    }
}
