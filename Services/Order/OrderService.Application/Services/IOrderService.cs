using System;
using System.Collections.Generic;
using System.Text;
using OrderService.Application.Models;

namespace OrderService.Application.Services
{
    public interface IOrderService
    {
        Task<IReadOnlyList<OrderDto>> GetForEmployeeAsync(Guid employeeId,CancellationToken cancellationToken = default);
        Task<OrderDto?> GetByIdAsync(Guid id,Guid employeeId, CancellationToken cancellationToken = default);
        Task<OrderDto?> CreateAsync(CreateOrderRequest request, Guid loggedInEmployeeId, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Guid id, UpdateOrderRequest request, Guid loggedInEmployeeId, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync( Guid id,Guid loggedInEmployeeId, CancellationToken cancellationToken = default);
    }
}
