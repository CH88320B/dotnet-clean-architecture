using CleanArch.Application.Common;
using CleanArch.Domain.Common;
using CleanArch.Domain.Entities;

namespace CleanArch.Application.Abstractions;

public interface IRepository<TEntity> where TEntity : Entity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(TEntity entity, CancellationToken ct = default);

    void Remove(TEntity entity);
}

public interface IProductRepository : IRepository<Product>
{
    Task<bool> SkuExistsAsync(string sku, CancellationToken ct = default);

    Task<PagedResult<Product>> SearchAsync(ProductQuery query, CancellationToken ct = default);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);

    Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct = default);

    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken ct = default);
}

/// <summary>Commits every change tracked by the repositories in a single transaction.</summary>
public interface IUnitOfWork
{
    IProductRepository Products { get; }

    ICategoryRepository Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
