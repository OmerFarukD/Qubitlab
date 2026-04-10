namespace Qubitlab.Abstractions.Results;

public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    IError Error { get; }
}

public interface IResult<out TValue> : IResult
{
    TValue Value { get; }
}