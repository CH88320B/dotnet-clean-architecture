using CleanArch.Domain.Common;

namespace CleanArch.Domain.Entities;

public class Product : Entity
{
    private Product()
    {
        Sku = Name = string.Empty;
    }

    public Product(string sku, string name, decimal price, int stock, Guid categoryId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("SKU is required.");
        }

        Sku = sku.Trim().ToUpperInvariant();
        Name = string.Empty;
        CategoryId = categoryId;
        Update(name, description, price, stock);
    }

    public string Sku { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public int Stock { get; private set; }

    public bool IsActive { get; private set; } = true;

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public void Update(string name, string? description, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        if (price < 0)
        {
            throw new DomainException("Price cannot be negative.");
        }

        if (stock < 0)
        {
            throw new DomainException("Stock cannot be negative.");
        }

        Name = name.Trim();
        Description = description?.Trim();
        Price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
        Stock = stock;
    }

    public void MoveToCategory(Guid categoryId) => CategoryId = categoryId;

    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be positive.");
        }

        if (quantity > Stock)
        {
            throw new DomainException($"Insufficient stock for {Sku}: available {Stock}, requested {quantity}.");
        }

        Stock -= quantity;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be positive.");
        }

        Stock += quantity;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
