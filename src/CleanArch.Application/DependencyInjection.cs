using CleanArch.Application.Categories;
using CleanArch.Application.Common;
using CleanArch.Application.Products;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArch.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
