namespace WebApp.Blazor.Services
{
    public class AuthState
    {
        public string? Token { get; private set; }
        public Guid? EmployeeId { get; private set; }
        public string? Email { get; private set; }

        public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

        public event Action? OnChange;

        public void SetAuth(string token, Guid employeeId, string email)
        {
            Token = token;
            EmployeeId = employeeId;
            Email = email;
            OnChange?.Invoke();
        }

        public void Clear()
        {
            Token = null;
            EmployeeId = null;
            Email = null;
            OnChange?.Invoke();
        }
    }
}
