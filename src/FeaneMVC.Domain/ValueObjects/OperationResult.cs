namespace FeaneMVC.Domain.ValueObjects;

public class OperationResult
{
    public bool Status { get; set; }
    public string? Message { get; set; }

    public static OperationResult Success(string? message = null) => new()
    {
        Status = true,
        Message = message
    };

    public static OperationResult Failure(string message) => new()
    {
        Status = false,
        Message = message
    };
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; set; }

    public static OperationResult<T> Success(T? data = default, string? message = null) => new()
    {
        Status = true,
        Message = message,
        Data = data
    };

    public new static OperationResult<T> Failure(string message) => new()
    {
        Status = false,
        Message = message
    };
}
