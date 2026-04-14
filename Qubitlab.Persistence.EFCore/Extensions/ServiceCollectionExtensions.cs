using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Qubitlab.Abstractions.Security;
using Qubitlab.Persistence.EFCore.Configurations;
using Qubitlab.Persistence.EFCore.Interceptors;
using Qubitlab.Persistence.EFCore.Services;
using Qubitlab.Persistence.EFCore.UnitOfWorkPattern;

namespace Qubitlab.Persistence.EFCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static QubitRepositoriesBuilder<TContext> AddQubitRepositories<TContext>(
        this IServiceCollection services,
        Action<QubitRepositoriesOptions>? configureOptions = null)
        where TContext : DbContext
    {
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

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddScoped<ICurrentUserService, DefaultCurrentUserService>();

        if (options.EnableAuditLogging)
        {
            services.TryAddScoped<AuditLogInterceptor>();
        }

        services.AddDbContext<TContext>((serviceProvider, dbOptions) =>
        {
            foreach (var configurator in options.DbContextConfigurators)
            {
                configurator(serviceProvider, dbOptions);
            }

            var interceptors = new List<IInterceptor>();

            if (options.EnableAuditLogging)
            {
                interceptors.Add(serviceProvider.GetRequiredService<AuditLogInterceptor>());
            }

            foreach (var interceptorType in options.CustomInterceptors)
            {
                var interceptor = (IInterceptor)serviceProvider.GetRequiredService(interceptorType);
                interceptors.Add(interceptor);
            }

            if (interceptors.Any())
            {
                dbOptions.AddInterceptors(interceptors.ToArray());
            }
        });

        services.TryAddScoped<IUnitOfWork>(sp =>
            new UnitOfWork<TContext>(sp.GetRequiredService<TContext>()));

        return new QubitRepositoriesBuilder<TContext>(services, options);
    }

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
