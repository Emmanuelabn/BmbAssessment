using System.Net;
using System.Text.Json;
using Serilog;

namespace OrderService.Application.External
{
    public class ProductReadServiceHttpClient : IProductReadService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ProductReadServiceHttpClient(
            HttpClient httpClient,
            ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<decimal?> GetProductPriceAsync(
            Guid productId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"api/products/{productId}";

                using var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.Warning("Product {ProductId} not found in ProductService", productId);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                var product = await JsonSerializer.DeserializeAsync<ProductResponse>(
                    stream,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    },
                    cancellationToken);

                if (product is null)
                {
                    _logger.Warning("ProductService returned empty body for product {ProductId}", productId);
                    return null;
                }

                return product.Price;
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Error calling ProductService for product {ProductId}",
                    productId);
                // In case of error we return null so Application can fail gracefully.
                return null;
            }
        }

        // DTO that matches ProductService response { id, name, price }
        private sealed class ProductResponse
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = default!;
            public decimal Price { get; set; }
        }
    }
}
