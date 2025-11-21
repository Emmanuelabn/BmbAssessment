using OrderService.Domain.Entities;

namespace OrderService.Domain.Repositories
{
    // in the domain layer, IProductRepository is defined as interface to abstract data access logic,
    // this allows the domain modl to stay independent of any specific data storage implementations
    //(example: mssql, postgresql.....)
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Order>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Order order, CancellationToken cancellationToken = default);
        Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
        Task DeleteAsync(Order order, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
