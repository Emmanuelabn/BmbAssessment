using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrderService.Application.Models
{
    public sealed class OrderDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
        public decimal Total { get; init; }
        public Guid ClientId { get; init; }
        public DateTime OrderDate { get; init; }
        public Guid LoggedInEmployeeId { get; init; }
    }

    public sealed class CreateOrderRequest
    {
        [Required]
        public Guid ProductId { get; init; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; init; }

        [Required]
        public Guid ClientId { get; init; }

        public DateTime? OrderDate { get; init; }
    }

    public sealed class UpdateOrderRequest
    {
        [Required]
        public Guid ProductId { get; init; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; init; }

        [Required]
        public Guid ClientId { get; init; }

        public DateTime? OrderDate { get; init; }
    }
}
