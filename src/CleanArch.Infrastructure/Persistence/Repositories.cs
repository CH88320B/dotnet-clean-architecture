using System.Linq.Expressions;
using CleanArch.Application.Abstractions;
using CleanArch.Application.Common;
using CleanArch.Domain.Common;
using CleanArch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArch.Infrastructure.Persistence;

internal class Repository<TEntity>(AppDbContext db) : IRepository<TEntity> where TEntity : Entity
{
    protected AppDbContext Db { get; } = db;

    public virtual Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Db.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default) =>
        await Db.Set<TEntity>().AddAsync(entity, ct);

    public void Remove(TEntity entity) => Db.Set<TEntity>().Remove(entity);
}

internal sealed class ProductRepository(AppDbContext db) : Repository<Product>(db), IProductRepository
{
    private static readonly Dictionary<string, Expression<Func<Product, object>>> SortColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = p => p.Name,
            ["price"] = p => p.Price,
            ["stock"] = p => p.Stock,
            ["createdAt"] = p => p.CreatedAtUtc,
        };

    public override Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> SkuExistsAsync(string sku, CancellationToken ct = default) =>
        Db.Products.AnyAsync(p => p.Sku == sku, ct);

    public async Task<PagedResult<Product>> SearchAsync(ProductQuery query, CancellationToken ct = default)
    {
        IQueryable<Product> products = Db.Products.AsNoTracking().Include(p => p.Category);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            products = products.Where(p => p.Name.Contains(term) || p.Sku.Contains(term));
        }

        if (query.CategoryId is { } categoryId)
        {
            products = products.Where(p => p.CategoryId == categoryId);
        }

        if (query.MinPrice is { } min)
        {
            products = products.Where(p => p.Price >= min);
        }

        if (query.MaxPrice is { } max)
        {
            products = products.Where(p => p.Price <= max);
        }

        if (query.InStock is { } inStock)
        {
            products = inStock ? products.Where(p => p.Stock > 0) : products.Where(p => p.Stock == 0);
        }

        var total = await products.CountAsync(ct);

        var sortKey = SortColumns.GetValueOrDefault(query.SortBy, SortColumns["createdAt"]);
        var ordered = query.SortDirection == SortDirection.Asc
            ? products.OrderBy(sortKey).ThenBy(p => p.Id)
            : products.OrderByDescending(sortKey).ThenBy(p => p.Id);

        var items = await ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Product>(items, query.Page, query.PageSize, total);
    }
}

internal sealed class CategoryRepository(AppDbContext db) : Repository<Category>(db), ICategoryRepository
{
    public Task<bool> NameExistsAsync(string name, CancellationToken ct = default) =>
        Db.Categories.AnyAsync(c => c.Name == name, ct);

    public async Task<IReadOnlyList<Category>> ListAsync(CancellationToken ct = default) =>
        await Db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public Task<bool> HasProductsAsync(Guid categoryId, CancellationToken ct = default) =>
        Db.Products.AnyAsync(p => p.CategoryId == categoryId, ct);
}

internal sealed class UnitOfWork(AppDbContext db, IProductRepository products, ICategoryRepository categories) : IUnitOfWork
{
    public IProductRepository Products { get; } = products;

    public ICategoryRepository Categories { get; } = categories;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("The resource was modified by another request. Reload and try again.");
        }
    }
}
