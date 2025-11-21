using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
        private readonly AuthState _authState;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiGatewayClient(HttpClient http, AuthState authState)
        {
            _http = http;
            _authState = authState;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string uri, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, uri);

            if (!string.IsNullOrEmpty(_authState.Token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _authState.Token);
            }

            request.Headers.Add("X-Employee-Id", _authState.EmployeeId.ToString());

            if (content != null)
            {
                request.Content = content;
            }

            Console.WriteLine($"[ApiGatewayClient] {method} {uri}, hasToken={(!string.IsNullOrEmpty(_authState.Token))}");

            return request;
        }

        // ===== PRODUCTS CRUD =====

        public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(HttpMethod.Get, "api/products");
            using var response = await _http.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var products = await response.Content
                .ReadFromJsonAsync<List<ProductDto>>(JsonOptions, cancellationToken);

            return products ?? new List<ProductDto>();
        }

        public async Task<ProductDto?> GetProductAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(HttpMethod.Get, $"api/products/{id}");
            using var response = await _http.SendAsync(request, cancellationToken);

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
            var json = JsonSerializer.Serialize(model, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = CreateRequest(HttpMethod.Post, "api/products", content);
            using var response = await _http.SendAsync(request, cancellationToken);

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
            var json = JsonSerializer.Serialize(model, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = CreateRequest(HttpMethod.Put, $"api/products/{id}", content);
            using var response = await _http.SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProductAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(HttpMethod.Delete, $"api/products/{id}");
            using var response = await _http.SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode;
        }

        // ===== ORDERS CRUD =====

        public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(HttpMethod.Get, "api/orders");
            using var response = await _http.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var orders = await response.Content
                .ReadFromJsonAsync<List<OrderDto>>(JsonOptions, cancellationToken);

            return orders ?? new List<OrderDto>();
        }

        public async Task<OrderDto?> GetOrderAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(HttpMethod.Get, $"api/orders/{id}");
            using var response = await _http.SendAsync(request, cancellationToken);

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
            var json = JsonSerializer.Serialize(model, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = CreateRequest(HttpMethod.Post, "api/orders", content);
            using var response = await _http.SendAsync(request, cancellationToken);

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
            var json = JsonSerializer.Serialize(model, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = CreateRequest(HttpMethod.Put, $"api/orders/{id}", content);
            using var response = await _http.SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteOrderAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(HttpMethod.Delete, $"api/orders/{id}");
            using var response = await _http.SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode;
        }
    }
}
