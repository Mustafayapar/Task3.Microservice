using MediatR;
using Product.Api.Application.Abstractions;
using Product.Api.Application.Commands;
using Product.Api.Domain.Entities;

namespace Product.Api.Application.Handlers
{
    public sealed class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _repo;
        private readonly IEventPublisher _bus;
        private readonly ICacheService _cache;

        public CreateProductHandler(IProductRepository repo, IEventPublisher bus, ICacheService cache)
        {
            _repo = repo;
            _bus = bus;
            _cache = cache;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
        {
            var entity = new ProductEntity
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock
            };

            await _repo.AddAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);

            // Event publish
            await _bus.PublishAsync(new ProductCreatedEvent(entity.Id, entity.Name, entity.Price, entity.Stock), ct);

            // Cache invalidation (liste cache'i bozalım)
            await _cache.RemoveAsync("products:all", ct);

            return entity.Id;
        }
    }
}
public sealed record ProductCreatedEvent(Guid Id, string Name, decimal Price, int Stock);

