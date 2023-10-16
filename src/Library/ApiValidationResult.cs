using Library;

/// <summary></summary>
public class ApiValidationResult
{
    /// <summary></summary>
    public ApiValidationResult(ApiValidationResultStatus status)
    {
        Status = status;
        HasErrors = status != ApiValidationResultStatus.Valid;
        this.ValidationErrors = Status is IApiValidationResultStatusErrors statusErrors ? statusErrors.ValidationErrors ?? new() : new();
    }

    /// <summary>
    /// Status of the validation result. Allowed values are Valid, NotFound and Invalid
    /// </summary>
    public ApiValidationResultStatus Status { get; protected set; }

    /// <summary>
    /// bool indicating if the result contains errors
    /// </summary>
    public bool HasErrors { get; }

    /// <summary>
    /// List of the errors returned from the validation method
    /// </summary>
    public List<ApiValidationError> ValidationErrors { get; init; }

    /// <summary></summary>
    public static ApiValidationResult NotFound(string message)
    {
        return new ApiValidationResult(ApiValidationResultStatus.NotFound with { ValidationErrors = message.IsTrimmedNullOrEmpty() ? new() : new() { new ApiValidationError { Message = message }  } });
    }

    public static ApiValidationResult NotFound(IEnumerable<ApiValidationError> validationErrors)
    {
        return new ApiValidationResult(ApiValidationResultStatus.NotFound with { ValidationErrors = validationErrors.ToList() });
    }

    /// <summary></summary>
    public static ApiValidationResult Valid()
    {
        return new ApiValidationResult(ApiValidationResultStatus.Valid);
    }

    /// <summary></summary>
    public static ApiValidationResult Invalid(IEnumerable<ApiValidationError> validationErrors)
    {
        return new ApiValidationResult(ApiValidationResultStatus.Invalid with { ValidationErrors = validationErrors.ToList() });
    }

    /// <summary></summary>
    public static ApiValidationResult Invalid(string message, string? propertyName = null)
    {
        return Invalid(new List<ApiValidationError> { new ApiValidationError { Message = message, PropertyName = propertyName } });
    }

    public static ApiValidationResult Forbidden(string message = "")
    {
        return new ApiValidationResult(ApiValidationResultStatus.Forbidden with { ValidationErrors = message.IsTrimmedNullOrEmpty() ? new() : new () { new ApiValidationError { Message = message } } });
    }

    public static ApiValidationResult Forbidden(IEnumerable<ApiValidationError> validationErrors)
    {
        return new ApiValidationResult(ApiValidationResultStatus.Forbidden with { ValidationErrors = validationErrors.ToList() });
    }

    public static ApiValidationResult Unavailable(string message = "")
    {
        return new ApiValidationResult(ApiValidationResultStatus.Unavailable with { ValidationErrors = message.IsTrimmedNullOrEmpty() ? new() : new() { new ApiValidationError { Message = message } } });
    }

    public static ApiValidationResult Unavailable(IEnumerable<ApiValidationError> validationErrors)
    {
        return new ApiValidationResult(ApiValidationResultStatus.Unavailable with { ValidationErrors = validationErrors.ToList() });
    }

    public static ApiValidationResult TooManyRequests(TimeSpan? retryAfter = null)
    {
        return new ApiValidationResult(ApiValidationResultStatus.TooManyRequests with { RetryAfter = retryAfter });
    }

    public static ApiValidationResult<T> Valid<T>(T data) => ApiValidationResult<T>.Valid(data);
    public static ApiValidationResult<T> Invalid<T>(IEnumerable<ApiValidationError> validationErrors) => ApiValidationResult<T>.Invalid(validationErrors);
    public static ApiValidationResult<T> Invalid<T>(string message, string? propertyName = null) => ApiValidationResult<T>.Invalid(message, propertyName);
    public static ApiValidationResult<T> NotFound<T>(string message) => ApiValidationResult<T>.NotFound(message);
    public static ApiValidationResult<T> Forbidden<T>(string message = "") => ApiValidationResult<T>.Forbidden();

    public static implicit operator ApiValidationResult(ApiValidationResultStatus status) => new ApiValidationResult(status);
    public static implicit operator ApiValidationResult((ApiValidationResultStatus status, List<ApiValidationError> errors) statusAndError)
    {
        return statusAndError.status switch
        {
            ApiValidationResultStatus.ForbiddenStatus => Forbidden(statusAndError.errors),
            ApiValidationResultStatus.InvalidStatus => Invalid(statusAndError.errors),
            ApiValidationResultStatus.NotFoundStatus => NotFound(statusAndError.errors),
            ApiValidationResultStatus.TooManyRequestsStatus => TooManyRequests(),
            ApiValidationResultStatus.UnavailableStatus => Unavailable(statusAndError.errors),
            ApiValidationResultStatus.ValidStatus => throw new Exception(""),
            _ => throw new NotImplementedException("")
        };
    }
    public static implicit operator ApiValidationResult((ApiValidationResultStatus status, string message) statusAndMessage)
    {
        return statusAndMessage.status switch
        {
            ApiValidationResultStatus.ForbiddenStatus => Forbidden(statusAndMessage.message),
            ApiValidationResultStatus.InvalidStatus => Invalid(statusAndMessage.message),
            ApiValidationResultStatus.NotFoundStatus => NotFound(statusAndMessage.message),
            ApiValidationResultStatus.TooManyRequestsStatus => TooManyRequests(),
            ApiValidationResultStatus.UnavailableStatus => Unavailable(statusAndMessage.message),
            ApiValidationResultStatus.ValidStatus => throw new Exception(""),
            _ => throw new NotImplementedException("")
        };
    }


    public void Deconstruct(out ApiValidationResultStatus status, out List<ApiValidationError> apiValidationErrors)
    {
        status = this.Status;
        apiValidationErrors = this.ValidationErrors;
    }
}

