using ProductService.Application.Models;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mappers
{
    public static class ProductMapper
    {
        public static ProductDto ToDto(this Product product) =>
            new()
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price
            };
    }
}
