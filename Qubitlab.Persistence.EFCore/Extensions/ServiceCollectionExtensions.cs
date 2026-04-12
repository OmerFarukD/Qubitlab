using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Qubitlab.Persistence.EFCore.Configurations;
using Qubitlab.Persistence.EFCore.Interceptors;
using Qubitlab.Persistence.EFCore.Services;
using Qubitlab.Persistence.EFCore.UnitOfWorkPattern;

namespace Qubitlab.Persistence.EFCore.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// QubitTech.Repositories servislerini ekler (Fluent API)
    /// </summary>
    public static QubitRepositoriesBuilder<TContext> AddQubitRepositories<TContext>(
        this IServiceCollection services,
        Action<QubitRepositoriesOptions>? configureOptions = null)
        where TContext : DbContext
    {
        // Options pattern setup
        var options = new QubitRepositoriesOptions();
        configureOptions?.Invoke(options);
        
        services.Configure<QubitRepositoriesOptions>(opts =>
        {
            opts.EnableAuditLogging = options.EnableAuditLogging;
            opts.EnableSoftDelete = options.EnableSoftDelete;
            opts.AutoSaveChanges = options.AutoSaveChanges;
            opts.DefaultPageSize = options.DefaultPageSize;
            opts.MaxPageSize = options.MaxPageSize;
        });

        // Core services
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddScoped<ICurrentUserService, CurrentUserService>();

        // Interceptors (conditional registration)
        if (options.EnableSoftDelete || options.EnableAuditLogging)
        {
            services.TryAddScoped<AuditInterceptor>();
        }
        
        if (options.EnableAuditLogging)
        {
            services.TryAddScoped<AuditLogInterceptor>();
        }

        // DbContext with interceptors
        services.AddDbContext<TContext>((serviceProvider, dbOptions) =>
        {
            // User configurations
            foreach (var configurator in options.DbContextConfigurators)
            {
                configurator(serviceProvider, dbOptions);
            }

            // Collect all interceptors
            var interceptors = new List<IInterceptor>();
            
            // Built-in interceptors
            if (options.EnableSoftDelete || options.EnableAuditLogging)
            {
                interceptors.Add(serviceProvider.GetRequiredService<AuditInterceptor>());
            }
            
            if (options.EnableAuditLogging)
            {
                interceptors.Add(serviceProvider.GetRequiredService<AuditLogInterceptor>());
            }

            // 🆕 Custom interceptors (from AddInterceptor<T>())
            foreach (var interceptorType in options.CustomInterceptors)
            {
                var interceptor = (IInterceptor)serviceProvider.GetRequiredService(interceptorType);
                interceptors.Add(interceptor);
            }

            // Add all interceptors to DbContext
            if (interceptors.Any())
            {
                dbOptions.AddInterceptors(interceptors.ToArray());
            }
        });

        // Unit of Work
        services.TryAddScoped<IUnitOfWork>(sp => 
            new UnitOfWork<TContext>(sp.GetRequiredService<TContext>()));

        return new QubitRepositoriesBuilder<TContext>(services, options);
    }

    /// <summary>
    /// IConfiguration'dan otomatik bind ederek QubitRepositories servislerini ekler
    /// </summary>
    public static QubitRepositoriesBuilder<TContext> AddQubitRepositories<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "QubitRepositories")
        where TContext : DbContext
    {
        return services.AddQubitRepositories<TContext>(options =>
        {
            configuration.GetSection(sectionName).Bind(options);
        });
    }
}