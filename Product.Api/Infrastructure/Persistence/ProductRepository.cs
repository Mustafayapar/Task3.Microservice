using Microsoft.EntityFrameworkCore;
using Product.Api.Application.Abstractions;
using Product.Api.Domain.Entities;

namespace Product.Api.Infrastructure.Persistence
{
    public class ProductRepository:IProductRepository
    {
        private readonly ProductDbContext _db;
        public ProductRepository(ProductDbContext db) => _db = db;

        public Task AddAsync(ProductEntity entity, CancellationToken ct = default)
            => _db.Products.AddAsync(entity, ct).AsTask();

        public Task UpdateAsync(ProductEntity entity, CancellationToken ct = default)
        {
            _db.Products.Update(entity);
            return Task.CompletedTask;
        }

        public Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task<IReadOnlyList<ProductEntity>> GetAllAsync(CancellationToken ct = default)
            => await _db.Products.AsNoTracking().ToListAsync(ct);

        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
