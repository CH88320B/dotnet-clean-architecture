using AutoMapper;
using CleanArch.Application.Categories;
using CleanArch.Application.Products;
using CleanArch.Domain.Entities;

namespace CleanArch.Application.Common;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForCtorParam(nameof(ProductDto.CategoryName), opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

        CreateMap<Category, CategoryDto>();
    }
}
