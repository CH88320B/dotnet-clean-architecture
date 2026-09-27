namespace CleanArch.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    bool IsActive,
    Guid CategoryId,
    string? CategoryName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid CategoryId);

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid CategoryId);

public sealed record AdjustStockRequest(int Quantity);
