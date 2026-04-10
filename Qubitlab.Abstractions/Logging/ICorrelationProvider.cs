namespace Qubitlab.Abstractions.Logging;

public interface ICorrelationProvider
{
    string CorrelationId { get; }
    void Set(string correlationId);
}