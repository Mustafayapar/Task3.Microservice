using MediatR;
using Product.Api.Domain.Entities;

namespace Product.Api.Application.Queries
{
    public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductEntity?>;

}
