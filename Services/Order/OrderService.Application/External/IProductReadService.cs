namespace OrderService.Application.External
{
    // IProductReadService is abstracting read operations for products from Product microservice
    // so we can get product price when creating an order through http or mq or any other way
    public interface IProductReadService
    {
        Task<decimal?> GetProductPriceAsync(Guid productId, CancellationToken cancellationToken = default);
    }
}
