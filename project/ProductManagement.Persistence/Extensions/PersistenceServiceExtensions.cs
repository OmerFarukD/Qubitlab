using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;
using ProductManagement.Persistence.Context;
using ProductManagement.Persistence.Repositories;
using Qubitlab.Identity.Extensions;

namespace ProductManagement.Persistence.Extensions;

public static class PersistenceServiceExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' bulunamadı. " +
                $"appsettings.json dosyasını kontrol edin.");

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        Action<DbContextOptionsBuilder> configureDbContext = options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });
        };

        services.AddDbContext<AppDbContext>(configureDbContext);

        services.AddQubitlabIdentity<AppDbContext, User>();

        return services;
    }
}
