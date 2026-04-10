namespace Qubitlab.Abstractions.Logging;

public interface ICorrelationIdProvider
{
    string CorrelationId { get; }
    void Set(string correlationId);
}