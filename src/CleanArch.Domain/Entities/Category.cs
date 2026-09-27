using CleanArch.Domain.Common;

namespace CleanArch.Domain.Entities;

public class Category : Entity
{
    private readonly List<Product> _products = [];

    private Category()
    {
        Name = string.Empty;
    }

    public Category(string name)
    {
        Name = string.Empty;
        Rename(name);
    }

    public string Name { get; private set; }

    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.");
        }

        Name = name.Trim();
    }
}
