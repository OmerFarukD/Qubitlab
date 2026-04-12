using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Qubitlab.Persistence.EFCore.Configurations;

public sealed class QubitRepositoriesBuilder<TContext> where TContext : DbContext
{
    private readonly IServiceCollection _services;
    private readonly QubitRepositoriesOptions _options;

    internal QubitRepositoriesBuilder(
        IServiceCollection services, 
        QubitRepositoriesOptions options)
    {
        _services = services;
        _options = options;
    }
    
    public QubitRepositoriesBuilder<TContext> ConfigureDbContext(
        Action<IServiceProvider, DbContextOptionsBuilder> configure)
    {
        _options.DbContextConfigurators.Add(configure);
        return this;
    }


    public QubitRepositoriesBuilder<TContext> DisableAuditLogging()
    {
        _options.EnableAuditLogging = false;
        return this;
    }


    public QubitRepositoriesBuilder<TContext> DisableSoftDelete()
    {
        _options.EnableSoftDelete = false;
        return this;
    }


    public QubitRepositoriesBuilder<TContext> DisableAutoSaveChanges()
    {
        _options.AutoSaveChanges = false;
        return this;
    }
    
    public QubitRepositoriesBuilder<TContext> AddInterceptor<TInterceptor>()
        where TInterceptor : class, IInterceptor
    {
        _services.AddScoped<TInterceptor>();
        _options.CustomInterceptors.Add(typeof(TInterceptor));
        return this;
    }


    public QubitRepositoriesBuilder<TContext> WithDefaultPageSize(int pageSize)
    {
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0");
            
        _options.DefaultPageSize = pageSize;
        return this;
    }
    
    public QubitRepositoriesBuilder<TContext> WithMaxPageSize(int maxSize)
    {
        if (maxSize < 1)
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Max page size must be greater than 0");
            
        _options.MaxPageSize = maxSize;
        return this;
    }
}