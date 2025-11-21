namespace Shared.Contracts.Dtos
{
    public record LoginDto(string Token, string EmployeeName, Guid EmployeeId);
}
