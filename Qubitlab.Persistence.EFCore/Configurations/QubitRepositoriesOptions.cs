using Microsoft.EntityFrameworkCore;

namespace Qubitlab.Persistence.EFCore.Configurations;

public sealed class QubitRepositoriesOptions
{
    /// <summary>
    /// Audit logging açık mı? (AuditLogInterceptor)
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;
    
    /// <summary>
    /// Otomatik soft delete ve audit interceptor açık mı? (AuditInterceptor)
    /// </summary>
    public bool EnableSoftDelete { get; set; } = true;
    
    /// <summary>
    /// Repository'lerde otomatik SaveChanges çağrılsın mı?
    /// false ise Unit of Work ile manuel kontrol edilmelidir.
    /// </summary>
    public bool AutoSaveChanges { get; set; } = true;
    
    /// <summary>
    /// Varsayılan sayfalama boyutu
    /// </summary>
    public int DefaultPageSize { get; set; } = 10;
    
    /// <summary>
    /// Maksimum sayfalama boyutu (güvenlik için)
    /// </summary>
    public int MaxPageSize { get; set; } = 100;
    
    /// <summary>
    /// DbContext konfigüratörleri (internal)
    /// </summary>
    internal List<Action<IServiceProvider, DbContextOptionsBuilder>> DbContextConfigurators { get; } = new();
    
    /// <summary>
    /// Kayıtlı custom interceptor tipleri (internal)
    /// </summary>
    internal List<Type> CustomInterceptors { get; } = new();
}