using System;
using System.Collections.Generic;
using System.Text;
using OrderService.Application.External;
using OrderService.Application.Mappers;
using OrderService.Application.Models;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductReadService _productReadService;

        public OrderService(
            IOrderRepository orderRepository,
            IProductReadService productReadService)
        {
            _orderRepository = orderRepository;
            _productReadService = productReadService;
        }

        public async Task<IReadOnlyList<OrderDto>> GetForEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var orders = await _orderRepository.GetByEmployeeAsync(employeeId, cancellationToken);
            return orders.Select(o => o.ToDto()).ToList();
        }

        public async Task<OrderDto?> GetByIdAsync(Guid id, Guid employeeId, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

            if (order is null || order.LoggedInEmployeeId != employeeId)
                return null;

            return order.ToDto();
        }

        public async Task<OrderDto?> CreateAsync(CreateOrderRequest request, Guid loggedInEmployeeId, CancellationToken cancellationToken = default)
        {
            var price = await _productReadService.GetProductPriceAsync(request.ProductId, cancellationToken);
            if (price is null)
            {
                // Product is not found here so we can't create order
                return null;
            }

            var total = request.Quantity * price.Value;

            var orderDate = request.OrderDate ?? DateTime.UtcNow;

            var order = new Order();

            order.ProductId = request.ProductId;
            order.Quantity = request.Quantity;
            order.Total = total;
            order.ClientId = request.ClientId;
            order.OrderDate = orderDate;
            order.LoggedInEmployeeId = loggedInEmployeeId;

            await _orderRepository.AddAsync(order, cancellationToken);
            
            return order.ToDto();
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateOrderRequest request, Guid loggedInEmployeeId, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

            // if the order is not found or the logged in employee is not the owner of the order return false
            if (order is null || order.LoggedInEmployeeId != loggedInEmployeeId) return false;

            var price = await _productReadService.GetProductPriceAsync(request.ProductId, cancellationToken);
            if (price is null)
            {
                // Product is not found here so we can't update order
                return false;
            }

            var total = request.Quantity * price.Value;

            order.ProductId = request.ProductId;
            order.Quantity = request.Quantity;
            order.Total = total;
            order.ClientId = request.ClientId;
            order.OrderDate = request.OrderDate ?? order.OrderDate;

            await _orderRepository.UpdateAsync(order, cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid loggedInEmployeeId, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

            // if the order is not found or the logged in employee is not the owner of the order return false
            if (order is null || order.LoggedInEmployeeId != loggedInEmployeeId) return false;

            await _orderRepository.DeleteAsync(order, cancellationToken);
            return true;
        }
    }
}
