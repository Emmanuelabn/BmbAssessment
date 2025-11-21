using System.Net.Http.Json;
using System.Text.Json;

namespace WebApp.Blazor.Services
{
    public sealed class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    public class AuthClient
    {
        private readonly HttpClient _http;
        private readonly AuthState _authState;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthClient(HttpClient http, AuthState authState)
        {
            _http = http;
            _authState = authState;
        }

        public async Task<bool> RegisterAsync(string email, string password)
        {
            var request = new RegisterRequest { Email = email, Password = password };

            var response = await _http.PostAsJsonAsync("auth/register", request, JsonOptions);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var request = new LoginRequest { Email = email, Password = password };

            var response = await _http.PostAsJsonAsync("auth/login", request, JsonOptions);
            if (!response.IsSuccessStatusCode)
                return false;

            var loginResponse = await response.Content
                .ReadFromJsonAsync<LoginResponse>(JsonOptions);

            if (loginResponse is null || string.IsNullOrEmpty(loginResponse.Token))
                return false;

            _authState.SetAuth(loginResponse.Token, loginResponse.EmployeeId, loginResponse.Email);

            Console.WriteLine($"[AuthClient] Logged in, token length = {loginResponse.Token.Length}");

            return true;
        }

        public Task LogoutAsync()
        {
            _authState.Clear();
            return Task.CompletedTask;
        }
    }
}
