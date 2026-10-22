using System;

namespace SharedContracts.Responses;

public class ErrorModel
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;

    public ErrorModel() { }

    public ErrorModel(string propertyName, string errorMessage, string errorCode = "VALIDATION_ERROR")
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }
}
