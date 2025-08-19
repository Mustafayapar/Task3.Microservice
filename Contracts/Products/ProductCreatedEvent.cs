using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Products
{
    public sealed record ProductCreatedEvent(
    Guid Id,
    string Name,
    decimal Price,
    int Stock,
    DateTime CreatedAtUtc
);
}