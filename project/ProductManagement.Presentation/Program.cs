using Microsoft.OpenApi.Models;
using ProductManagement.Application.Extensions;
using ProductManagement.Persistence.Context;
using ProductManagement.Persistence.Extensions;
using Qubitlab.CrossCuttingConcerns.Exceptions.Extensions;
using Qubitlab.CrossCuttingConcerns.Extensions;
using Qubitlab.Logging.EFCore;
using Qubitlab.Logging.Serilog;
using Qubitlab.Security.Extensions;
using Qubitlab.Security.Hashing;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseQubitlabSerilog(builder.Configuration);
builder.Host.UseQubitlabEfCoreLogging<AppDbContext>(options =>
{
    options.MinimumLevel = Serilog.Events.LogEventLevel.Warning;
    options.BatchSizeLimit = 50;
    options.Period = TimeSpan.FromSeconds(5);
});

builder.Services.AddQubitlabSerilog(builder.Configuration);
builder.Services.AddQubitlabCorrelation();
builder.Services.AddApplicationDependencies();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddQubitlabSecurity(
    configureJwt: opt =>
    {
        opt.SecretKey = builder.Configuration["Jwt:SecretKey"]!;
        opt.Issuer = builder.Configuration["Jwt:Issuer"]!;
        opt.Audience = builder.Configuration["Jwt:Audience"]!;
        opt.AccessTokenExpirationMinutes = int.Parse(builder.Configuration["Jwt:AccessTokenExpirationMinutes"]!);
        opt.RefreshTokenExpirationDays = int.Parse(builder.Configuration["Jwt:RefreshTokenExpirationDays"]!);
    },
    algorithm: HashingAlgorithm.BCrypt,
    enableRevocation: false);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ProductManagement API",
        Version = "v1",
        Description = "ProductManagement servisi icin REST API dokumantasyonu.",
        Contact = new OpenApiContact
        {
            Name = "Qubitlab",
            Email = "info@qubitlab.io"
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token'inizi buraya girin."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        ui.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductManagement API v1");
        ui.RoutePrefix = string.Empty;
        ui.DisplayRequestDuration();
        ui.EnableDeepLinking();
    });
}

app.UseQubitlabCorrelation();
//app.UseQubitlabExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
