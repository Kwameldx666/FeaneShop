namespace ProductService.Domain.ValueObjects;

public class OperationResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public IDictionary<string, string[]>? Errors { get; init; }

    public static OperationResult Ok(string? message = null)
    {
        return new OperationResult
        {
            Success = true,
            Message = message
        };
    }

    public static OperationResult Fail(string message, IDictionary<string, string[]>? errors = null)
    {
        return new OperationResult
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; init; }

    public static OperationResult<T> Ok(T data, string? message = null)
    {
        return new OperationResult<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public new static OperationResult<T> Fail(string message, IDictionary<string, string[]>? errors = null)
    {
        return new OperationResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}