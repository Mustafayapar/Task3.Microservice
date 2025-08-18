using MediatR;

namespace Product.Api.Application.Commands
{
    public sealed record UpdateProductCommand(
    Guid Id, string Name, string? Description, decimal Price, int Stock
) : IRequest<bool>;
}
