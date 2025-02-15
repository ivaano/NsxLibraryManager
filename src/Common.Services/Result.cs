using System.Diagnostics.CodeAnalysis;

namespace Common.Services;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    private readonly T? _value;
    public string? Error { get; }

    internal Result(T? value, bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    public T Value => IsSuccess 
        ? _value ?? throw new InvalidOperationException("Unexpected null value in success state")
        : throw new InvalidOperationException("Cannot access Value in failure state");

    public Result<TResult> Map<TResult>(Func<T, TResult> mapper) =>
        IsSuccess
            ? Result.Success(mapper(Value))
            : Result.Failure<TResult>(Error!);

    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder) =>
        IsSuccess
            ? binder(Value)
            : Result.Failure<TResult>(Error!);

    // Optional methods for safer access
    public T? ValueOrDefault(T? defaultValue = default) =>
        IsSuccess ? _value : defaultValue;

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        value = _value;
        return IsSuccess;
    }
}

public static class Result
{
    public static Result<T> Success<T>(T value) =>
        new(value, true, null);

    public static Result<T> Failure<T>(string error) =>
        new(default, false, error);
}