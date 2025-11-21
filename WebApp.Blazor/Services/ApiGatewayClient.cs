using System.Net;
using System.Text;
using System.Text.Json;

namespace WebApp.Blazor.Services
{
    // ---------- DTOs / Models used by the frontend ----------

    public sealed class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public sealed class ProductUpsertRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public sealed class OrderDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public Guid ClientId { get; set; }
        public DateTime OrderDate { get; set; }
        public Guid LoggedInEmployeeId { get; set; }
    }

    public sealed class CreateOrderRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public Guid ClientId { get; set; }
        public DateTime? OrderDate { get; set; }
    }

    public sealed class UpdateOrderRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public Guid ClientId { get; set; }
        public DateTime? OrderDate { get; set; }
    }

    // ---------- Gateway client ----------

    public class ApiGatewayClient
    {
        private readonly HttpClient _http;

        // TODO: hook this to a real login later
        private static readonly Guid EmployeeId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiGatewayClient(HttpClient http)
        {
            _http = http;
        }

        // ===== PRODUCTS CRUD =====

        public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(
            CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("api/products", cancellationToken);
            response.EnsureSuccessStatusCode();

            var products = await response.Content
                .ReadFromJsonAsync<List<ProductDto>>(JsonOptions, cancellationToken);

            return products ?? new List<ProductDto>();
        }

        public async Task<ProductDto?> GetProductAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"api/products/{id}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<ProductDto>(JsonOptions, cancellationToken);
        }

        public async Task<ProductDto?> CreateProductAsync(
            ProductUpsertRequest model,
            CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("api/products", model, JsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<ProductDto>(JsonOptions, cancellationToken);
        }

        public async Task<bool> UpdateProductAsync(
            Guid id,
            ProductUpsertRequest model,
            CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"api/products/{id}", model, JsonOptions, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProductAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"api/products/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }

        // ===== ORDERS CRUD =====

        public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/orders");
            request.Headers.Add("X-Employee-Id", EmployeeId.ToString());

            var response = await _http.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var orders = await response.Content
                .ReadFromJsonAsync<List<OrderDto>>(JsonOptions, cancellationToken);

            return orders ?? new List<OrderDto>();
        }

        public async Task<OrderDto?> GetOrderAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/orders/{id}");
            request.Headers.Add("X-Employee-Id", EmployeeId.ToString());

            var response = await _http.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<OrderDto>(JsonOptions, cancellationToken);
        }

        public async Task<OrderDto?> CreateOrderAsync(
            CreateOrderRequest model,
            CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(model);
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/orders")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("X-Employee-Id", EmployeeId.ToString());

            var response = await _http.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<OrderDto>(JsonOptions, cancellationToken);
        }

        public async Task<bool> UpdateOrderAsync(
            Guid id,
            UpdateOrderRequest model,
            CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(model);
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/orders/{id}")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("X-Employee-Id", EmployeeId.ToString());

            var response = await _http.SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteOrderAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/orders/{id}");
            request.Headers.Add("X-Employee-Id", EmployeeId.ToString());

            var response = await _http.SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode;
        }
    }
}
