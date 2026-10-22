using System.Collections.Generic;

namespace SharedContracts.Responses;

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ErrorModel>? Errors { get; set; }
    public string TraceId { get; set; } = string.Empty;

    public static ApiResponse SuccessResult(string message = "Success") =>
        new() { Success = true, Message = message };

    public static ApiResponse FailureResult(List<ErrorModel> errors, string message = "Failure") =>
        new() { Success = false, Errors = errors, Message = message };

    public static ApiResponse FailureResult(string propertyName, string errorMessage, string errorCode = "ERROR", string message = "Failure") =>
        new()
        {
            Success = false,
            Errors = new List<ErrorModel> { new(propertyName, errorMessage, errorCode) },
            Message = message
        };
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "Success") =>
        new() { Success = true, Data = data, Message = message };

    public static new ApiResponse<T> FailureResult(List<ErrorModel> errors, string message = "Failure") =>
        new() { Success = false, Errors = errors, Message = message };

    public static new ApiResponse<T> FailureResult(string propertyName, string errorMessage, string errorCode = "ERROR", string message = "Failure") =>
        new()
        {
            Success = false,
            Errors = new List<ErrorModel> { new(propertyName, errorMessage, errorCode) },
            Message = message
        };
}
