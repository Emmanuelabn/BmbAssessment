using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Contracts.Dtos
{
    public record OrderDto(Guid Id, Guid ProductId, int Quantity, decimal Total, Guid ClientId, DateTime OrderDate);
}
