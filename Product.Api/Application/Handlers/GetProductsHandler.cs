using MediatR;
using Product.Api.Application.Abstractions;
using Product.Api.Application.Queries;
using Product.Api.Domain.Entities;

namespace Product.Api.Application.Handlers
{
    public class GetProductsHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductEntity>>
    {
        private readonly IProductRepository _repo;
        private readonly ICacheService _cache;

        public GetProductsHandler(IProductRepository repo, ICacheService cache)
        {
            _repo = repo; _cache = cache;
        }

        public async Task<IReadOnlyList<ProductEntity>> Handle(GetProductsQuery request, CancellationToken ct)
        {
            const string cacheKey = "products:all";
            var cached = await _cache.GetAsync<IReadOnlyList<ProductEntity>>(cacheKey, ct);
            if (cached is not null) return cached;

            var items = await _repo.GetAllAsync(ct);
            await _cache.SetAsync(cacheKey, items, TimeSpan.FromMinutes(5), ct); // TTL
            return items;
        }
    }
}
