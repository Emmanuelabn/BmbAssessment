using OrderService.Application.Models;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mappers
{
    public static class OrderMapper
    {
        public static OrderDto ToDto(this Order order) =>
            new()
            {
                Id = order.Id,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                Total = order.Total,
                ClientId = order.ClientId,
                OrderDate = order.OrderDate,
                LoggedInEmployeeId = order.LoggedInEmployeeId
            };
    }
}
