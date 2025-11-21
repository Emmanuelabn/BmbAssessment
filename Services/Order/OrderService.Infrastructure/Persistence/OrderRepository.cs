using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Infrastructure.Persistence
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _dbContext;

        public OrderRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Order>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.LoggedInEmployeeId == employeeId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _dbContext.Orders.AddAsync(order, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Order order, CancellationToken cancellationToken = default)
        {
            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Orders
                .AnyAsync(o => o.Id == id, cancellationToken);
        }
    }
}
