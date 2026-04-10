namespace Qubitlab.Abstractions.Time;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
    DateOnly Today { get; }
    DateTimeOffset UtcNowOffset { get; }
}