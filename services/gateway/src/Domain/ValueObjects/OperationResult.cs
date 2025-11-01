namespace AuthService.Domain.ValueObjects;

public class OperationResult
{
    public bool Status { get; set; }
    public string? Message { get; set; }

    public static OperationResult Success(string? message = null)
    {
        return new OperationResult
        {
            Status = true,
            Message = message
        };
    }

    public static OperationResult Failure(string message)
    {
        return new OperationResult
        {
            Status = false,
            Message = message
        };
    }
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; set; }

    public static OperationResult<T> Success(T? data = default, string? message = null)
    {
        return new OperationResult<T>
        {
            Status = true,
            Message = message,
            Data = data
        };
    }

    public new static OperationResult<T> Failure(string message)
    {
        return new OperationResult<T>
        {
            Status = false,
            Message = message
        };
    }
}