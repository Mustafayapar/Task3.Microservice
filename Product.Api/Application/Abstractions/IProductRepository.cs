
using Product.Api.Domain.Entities;

namespace Product.Api.Application.Abstractions
{
    public interface IProductRepository
    {
        Task AddAsync(ProductEntity entity, CancellationToken ct = default);
        Task UpdateAsync(ProductEntity entity, CancellationToken ct = default);
        Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<ProductEntity>> GetAllAsync(CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);

    }
}
