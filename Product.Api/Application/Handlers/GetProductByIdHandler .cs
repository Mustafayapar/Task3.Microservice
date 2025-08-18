using MediatR;
using Product.Api.Application.Abstractions;
using Product.Api.Application.Queries;
using Product.Api.Domain.Entities;

namespace Product.Api.Application.Handlers
{
    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductEntity?>
    {
        private readonly IProductRepository _repo;
        private readonly ICacheService _cache;

        public GetProductByIdHandler(IProductRepository repo, ICacheService cache)
        {
            _repo = repo; _cache = cache;
        }

        public async Task<ProductEntity?> Handle(GetProductByIdQuery request, CancellationToken ct)
        {
            var key = $"products:{request.Id}";
            var cached = await _cache.GetAsync<ProductEntity>(key, ct);
            if (cached is not null) return cached;

            var entity = await _repo.GetByIdAsync(request.Id, ct);
            if (entity is null) return null;

            await _cache.SetAsync(key, entity, TimeSpan.FromMinutes(10), ct);
            return entity;
        }
    }
}
