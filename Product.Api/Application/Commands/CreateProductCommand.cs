using MediatR;

namespace Product.Api.Application.Commands
{
    public sealed record CreateProductCommand(
    string Name, string? Description, decimal Price, int Stock
) : IRequest<Guid>;
    
}
