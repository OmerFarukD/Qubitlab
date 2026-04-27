using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductManagement.Application.Services.Auth;
using Qubitlab.Application.BaseBusiness;
using Qubitlab.Application.Pipelines.Authorization;
using Qubitlab.Application.Pipelines.Logging;
using Qubitlab.Application.Pipelines.Validation;

namespace ProductManagement.Application.Extensions;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddSubClassesOfType(Assembly.GetExecutingAssembly(), typeof(BaseBusinessRules));

        services.AddScoped<IAuthService, AuthService>();

        services.AddMediatR(opt =>
        {
            opt.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            opt.AddOpenBehavior(typeof(RequestValidationBehavior<,>));
            opt.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            opt.AddOpenBehavior(typeof(LoggingBehavior<,>));
            opt.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });
        return services;
    }
    
    public static IServiceCollection AddSubClassesOfType(
        this IServiceCollection services,
        Assembly assembly,
        Type type,
        Func<IServiceCollection, Type, IServiceCollection>? addWithLifeCycle = null
    )
    {
        var types = assembly.GetTypes().Where(t => t.IsSubclassOf(type) && type != t).ToList();
        foreach (var item in types)
            if (addWithLifeCycle == null)
                services.AddScoped(item);
            else
                addWithLifeCycle(services, type);
        return services;
    }
}
