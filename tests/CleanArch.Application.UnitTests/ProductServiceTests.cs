using AutoMapper;
using CleanArch.Application.Abstractions;
using CleanArch.Application.Common;
using CleanArch.Application.Products;
using CleanArch.Domain.Entities;
using FluentValidation;

namespace CleanArch.Application.UnitTests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _uow.SetupGet(u => u.Products).Returns(_products.Object);
        _uow.SetupGet(u => u.Categories).Returns(_categories.Object);

        var mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>()).CreateMapper();
        _sut = new ProductService(
            _uow.Object,
            mapper,
            new CreateProductRequestValidator(),
            new UpdateProductRequestValidator(),
            new AdjustStockRequestValidator(),
            new ProductQueryValidator());
    }

    [Fact]
    public void MappingConfiguration_IsValid()
    {
        new MapperConfiguration(c => c.AddProfile<MappingProfile>()).AssertConfigurationIsValid();
    }

    [Fact]
    public async Task Create_PersistsProduct_WhenValid()
    {
        var categoryId = Guid.NewGuid();
        _categories.Setup(c => c.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(new Category("Peripherals"));

        var dto = await _sut.CreateAsync(new CreateProductRequest("kb-1", "Keyboard", null, 49.9m, 3, categoryId));

        dto.Sku.Should().Be("KB-1");
        _products.Verify(p => p.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_Throws_WhenSkuAlreadyExists()
    {
        _products.Setup(p => p.SkuExistsAsync("KB-1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.CreateAsync(new CreateProductRequest("kb-1", "Keyboard", null, 1m, 1, Guid.NewGuid()));

        await act.Should().ThrowAsync<ConflictException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_Throws_WhenCategoryDoesNotExist()
    {
        var act = () => _sut.CreateAsync(new CreateProductRequest("kb-1", "Keyboard", null, 1m, 1, Guid.NewGuid()));

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Category*");
    }

    [Fact]
    public async Task Create_Throws_ValidationException_ForInvalidRequest()
    {
        var act = () => _sut.CreateAsync(new CreateProductRequest("bad sku!", "", null, -1m, -5, Guid.Empty));

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Select(e => e.PropertyName).Should()
            .Contain(new[] { "Sku", "Name", "Price", "Stock", "CategoryId" });
    }

    [Fact]
    public async Task Get_Throws_WhenMissing()
    {
        var act = () => _sut.GetAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(5, 15)]
    [InlineData(-4, 6)]
    public async Task AdjustStock_AddsOrRemoves(int delta, int expected)
    {
        var product = new Product("KB-1", "Keyboard", 1m, 10, Guid.NewGuid());
        _products.Setup(p => p.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var dto = await _sut.AdjustStockAsync(product.Id, new AdjustStockRequest(delta));

        dto.Stock.Should().Be(expected);
    }

    [Fact]
    public async Task Search_RejectsUnknownSortField()
    {
        var act = () => _sut.SearchAsync(new ProductQuery { SortBy = "password" });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Search_RejectsInvertedPriceRange()
    {
        var act = () => _sut.SearchAsync(new ProductQuery { MinPrice = 50, MaxPrice = 10 });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Search_MapsPagedResult()
    {
        var items = new List<Product> { new("A-1", "Alpha", 1m, 1, Guid.NewGuid()) };
        _products.Setup(p => p.SearchAsync(It.IsAny<ProductQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Product>(items, 1, 20, 41));

        var result = await _sut.SearchAsync(new ProductQuery());

        result.Items.Should().ContainSingle(p => p.Name == "Alpha");
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
    }
}
