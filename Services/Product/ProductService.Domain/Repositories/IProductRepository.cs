using System;
using System.Collections.Generic;
using System.Text;
using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories
{
    // in the domain layer, IProductRepository is defined as interface to abstract data access logic,
    // this allows the domain modl to stay independent of any specific data storage implementations
    //(example: mssql, postgresql.....)
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Product product, CancellationToken cancellationToken = default);
        Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
        Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
