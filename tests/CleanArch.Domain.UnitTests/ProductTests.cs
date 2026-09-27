using CleanArch.Domain.Common;
using CleanArch.Domain.Entities;

namespace CleanArch.Domain.UnitTests;

public class ProductTests
{
    private static Product NewProduct(int stock = 10, decimal price = 10m) =>
        new(" kb-001 ", "Keyboard", price, stock, Guid.NewGuid());

    [Fact]
    public void Constructor_NormalizesSku()
    {
        NewProduct().Sku.Should().Be("KB-001");
    }

    [Fact]
    public void Constructor_RoundsPriceToTwoDecimals()
    {
        NewProduct(price: 10.005m).Price.Should().Be(10.01m);
    }

    [Theory]
    [InlineData("", 1, 1)]
    [InlineData("Name", -1, 1)]
    [InlineData("Name", 1, -1)]
    public void Update_RejectsInvalidValues(string name, int price, int stock)
    {
        var product = NewProduct();

        var act = () => product.Update(name, null, price, stock);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveStock_DecrementsStock()
    {
        var product = NewProduct(stock: 5);

        product.RemoveStock(5);

        product.Stock.Should().Be(0);
    }

    [Fact]
    public void RemoveStock_FailsWhenInsufficient()
    {
        var product = NewProduct(stock: 2);

        var act = () => product.RemoveStock(3);

        act.Should().Throw<DomainException>().WithMessage("*Insufficient stock*");
        product.Stock.Should().Be(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void StockChanges_RequirePositiveQuantity(int quantity)
    {
        var product = NewProduct();

        product.Invoking(p => p.AddStock(quantity)).Should().Throw<DomainException>();
        product.Invoking(p => p.RemoveStock(quantity)).Should().Throw<DomainException>();
    }

    [Fact]
    public void Category_RequiresName()
    {
        var act = () => new Category("   ");

        act.Should().Throw<DomainException>();
    }
}
