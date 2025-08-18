using MediatR;
using Product.Api.Application.Abstractions;
using Product.Api.Application.Commands;

namespace Product.Api.Application.Handlers
{
    public sealed class UpdateProductHandler : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IProductRepository _repo;
        private readonly IEventPublisher _bus;
        private readonly ICacheService _cache;

        public UpdateProductHandler(IProductRepository repo, IEventPublisher bus, ICacheService cache)
        {
            _repo = repo; _bus = bus; _cache = cache;
        }

        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(request.Id, ct);
            if (entity is null) return false;

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.Price = request.Price;
            entity.Stock = request.Stock;
            entity.UpdatedAtUtc = DateTime.UtcNow;

            await _repo.UpdateAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);

            await _bus.PublishAsync(new ProductUpdatedEvent(entity.Id, entity.Name, entity.Price, entity.Stock), ct);

            // Cache invalidation: tekil + liste
            await _cache.RemoveAsync($"products:{entity.Id}", ct);
            await _cache.RemoveAsync("products:all", ct);

            return true;
        }
    }
}
public sealed record ProductUpdatedEvent(Guid Id, string Name, decimal Price, int Stock);

