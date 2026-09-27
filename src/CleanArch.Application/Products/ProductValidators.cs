using CleanArch.Application.Common;
using FluentValidation;

namespace CleanArch.Application.Products;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(40)
            .Matches("^[A-Za-z0-9-]+$").WithMessage("SKU may contain only letters, digits and hyphens.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).PrecisionScale(12, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).PrecisionScale(12, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public sealed class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.Quantity).NotEqual(0).InclusiveBetween(-10_000, 10_000);
    }
}

public sealed class ProductQueryValidator : AbstractValidator<ProductQuery>
{
    private static readonly string[] SortableFields = ["name", "price", "stock", "createdAt"];

    public ProductQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, ProductQuery.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue);
        RuleFor(x => x)
            .Must(x => x.MinPrice <= x.MaxPrice)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithName("MaxPrice")
            .WithMessage("MaxPrice must be greater than or equal to MinPrice.");
        RuleFor(x => x.SortBy)
            .Must(s => SortableFields.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"SortBy must be one of: {string.Join(", ", SortableFields)}.");
    }
}
