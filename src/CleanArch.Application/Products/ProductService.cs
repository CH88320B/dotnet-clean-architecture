using AutoMapper;
using CleanArch.Application.Abstractions;
using CleanArch.Application.Common;
using CleanArch.Domain.Entities;
using FluentValidation;

namespace CleanArch.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductDto>> SearchAsync(ProductQuery query, CancellationToken ct = default);

    Task<ProductDto> GetAsync(Guid id, CancellationToken ct = default);

    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);

    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);

    Task<ProductDto> AdjustStockAsync(Guid id, AdjustStockRequest request, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

internal sealed class ProductService(
    IUnitOfWork uow,
    IMapper mapper,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator,
    IValidator<AdjustStockRequest> stockValidator,
    IValidator<ProductQuery> queryValidator) : IProductService
{
    public async Task<PagedResult<ProductDto>> SearchAsync(ProductQuery query, CancellationToken ct = default)
    {
        await queryValidator.ValidateAndThrowAsync(query, ct);
        var page = await uow.Products.SearchAsync(query, ct);
        return page.Map(p => mapper.Map<ProductDto>(p));
    }

    public async Task<ProductDto> GetAsync(Guid id, CancellationToken ct = default) =>
        mapper.Map<ProductDto>(await FindAsync(id, ct));

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        if (await uow.Products.SkuExistsAsync(request.Sku.Trim().ToUpperInvariant(), ct))
        {
            throw new ConflictException($"A product with SKU '{request.Sku}' already exists.");
        }

        await EnsureCategoryExistsAsync(request.CategoryId, ct);

        var product = new Product(request.Sku, request.Name, request.Price, request.Stock, request.CategoryId, request.Description);
        await uow.Products.AddAsync(product, ct);
        await uow.SaveChangesAsync(ct);

        return mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var product = await FindAsync(id, ct);
        if (product.CategoryId != request.CategoryId)
        {
            await EnsureCategoryExistsAsync(request.CategoryId, ct);
            product.MoveToCategory(request.CategoryId);
        }

        product.Update(request.Name, request.Description, request.Price, request.Stock);
        await uow.SaveChangesAsync(ct);

        return mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> AdjustStockAsync(Guid id, AdjustStockRequest request, CancellationToken ct = default)
    {
        await stockValidator.ValidateAndThrowAsync(request, ct);

        var product = await FindAsync(id, ct);
        if (request.Quantity > 0)
        {
            product.AddStock(request.Quantity);
        }
        else
        {
            product.RemoveStock(-request.Quantity);
        }

        await uow.SaveChangesAsync(ct);
        return mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await FindAsync(id, ct);
        uow.Products.Remove(product);
        await uow.SaveChangesAsync(ct);
    }

    private async Task<Product> FindAsync(Guid id, CancellationToken ct) =>
        await uow.Products.GetByIdAsync(id, ct) ?? throw new NotFoundException("Product", id);

    private async Task EnsureCategoryExistsAsync(Guid categoryId, CancellationToken ct)
    {
        if (await uow.Categories.GetByIdAsync(categoryId, ct) is null)
        {
            throw new NotFoundException("Category", categoryId);
        }
    }
}
