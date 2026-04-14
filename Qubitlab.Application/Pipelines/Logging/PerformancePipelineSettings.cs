using Microsoft.Extensions.Options;

namespace Qubitlab.Application.Pipelines.Logging;

/// <summary>
/// <see cref="PerformanceBehavior{TRequest,TResponse}"/> için yapılandırma.
/// </summary>
public sealed class PerformancePipelineSettings
{
    /// <summary>
    /// Bu süreyi (ms) aşan request'ler Warning olarak loglanır.
    /// Varsayılan: <c>500</c> ms.
    /// </summary>
    public int SlowRequestThresholdMs { get; set; } = 500;
}
