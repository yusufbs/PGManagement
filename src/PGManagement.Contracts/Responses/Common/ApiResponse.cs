namespace PGManagement.Contracts.Responses.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public string? TraceId { get; init; }

    public ApiResponse() { }

    public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        TraceId = traceId
    };

    public static ApiResponse<T> Fail(string message, string? traceId = null) => new()
    {
        Success = false,
        Message = message,
        TraceId = traceId
    };
}

public class ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? TraceId { get; init; }

    public ApiResponse() { }

    public static ApiResponse Ok(string? message = null, string? traceId = null) => new()
    {
        Success = true,
        Message = message,
        TraceId = traceId
    };

    public static ApiResponse Fail(string message, string? traceId = null) => new()
    {
        Success = false,
        Message = message,
        TraceId = traceId
    };
}
