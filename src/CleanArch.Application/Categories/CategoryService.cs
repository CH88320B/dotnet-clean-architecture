using AutoMapper;
using CleanArch.Application.Abstractions;
using CleanArch.Application.Common;
using CleanArch.Domain.Entities;
using FluentValidation;

namespace CleanArch.Application.Categories;

public sealed record CategoryDto(Guid Id, string Name, DateTime CreatedAtUtc);

public sealed record CreateCategoryRequest(string Name);

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
    }
}

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken ct = default);

    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

internal sealed class CategoryService(
    IUnitOfWork uow,
    IMapper mapper,
    IValidator<CreateCategoryRequest> validator) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken ct = default) =>
        mapper.Map<IReadOnlyList<CategoryDto>>(await uow.Categories.ListAsync(ct));

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        if (await uow.Categories.NameExistsAsync(request.Name.Trim(), ct))
        {
            throw new ConflictException($"Category '{request.Name.Trim()}' already exists.");
        }

        var category = new Category(request.Name);
        await uow.Categories.AddAsync(category, ct);
        await uow.SaveChangesAsync(ct);
        return mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await uow.Categories.GetByIdAsync(id, ct) ?? throw new NotFoundException("Category", id);

        if (await uow.Categories.HasProductsAsync(id, ct))
        {
            throw new ConflictException("Cannot delete a category that still has products.");
        }

        uow.Categories.Remove(category);
        await uow.SaveChangesAsync(ct);
    }
}
