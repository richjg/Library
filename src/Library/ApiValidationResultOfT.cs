using Library;
using Newtonsoft.Json.Linq;

/// <summary></summary>
public class ApiValidationResult<T> : ApiValidationResult
{
    /// <summary></summary>
    public ApiValidationResult(ApiValidationResultStatus status) : base(status)
    {
        if (status is ApiValidationResultStatus.ValidStatus valid && valid.Data != null && valid.Data is not T)
        {
            if(valid.Data is JToken jToken)
            {
                valid = valid with { Data = jToken.ToObject<T>() };
                Status = status = valid;
            }
            else
            {
                throw new Exception($"ApiValidationResultStatusValid Expecting type {typeof(T)} got {valid.Data.GetType()}");
            }
        }

        this.Data = status is ApiValidationResultStatus.ValidStatus v ? (T?)v.Data : default;
    }

    /// <summary>
    /// Object returned from the method. Only populated if there are no errors
    /// </summary>
    public T? Data { get; init; }

    /// <summary></summary>
    public static ApiValidationResult<T> From(ApiValidationResult apiValidationResult)
    {
        return new ApiValidationResult<T>(apiValidationResult.Status);
    }

    /// <summary></summary>
    public new static ApiValidationResult<T> NotFound(string message = "")
    {
        return new ApiValidationResult<T>(ApiValidationResultStatus.NotFound with { ValidationErrors = new List<ApiValidationError> { new ApiValidationError { Message = message } } });
    }

    public new static ApiValidationResult<T> NotFound(IEnumerable<ApiValidationError> validationErrors)
    {
        return new ApiValidationResult<T>(ApiValidationResultStatus.NotFound with { ValidationErrors = validationErrors.ToList() });
    }

    /// <summary></summary>
    public static ApiValidationResult<T> Valid(T data)
    {
        return new ApiValidationResult<T>(ApiValidationResultStatus.Valid with { Data = data! });
    }

    /// <summary></summary>
    public static ApiValidationResult<T> Invalid(params ApiValidationError[] validationErrors)
    {
        return Invalid(validationErrors as IEnumerable<ApiValidationError>);
    }

    /// <summary></summary>
    public new static ApiValidationResult<T> Invalid(IEnumerable<ApiValidationError> validationErrors)
    {
        return new ApiValidationResult<T>(ApiValidationResultStatus.Invalid with { ValidationErrors = validationErrors.ToList() });
    }

    /// <summary></summary>
    public new static ApiValidationResult<T> Invalid(string message, string? propertyName = null)
    {
        return Invalid(new List<ApiValidationError> { new ApiValidationError { Message = message, PropertyName = propertyName } });
    }

    /// <summary></summary>
    public new static ApiValidationResult<T> Forbidden(string message = "")
    {
        return new ApiValidationResult<T>(ApiValidationResultStatus.Forbidden with { ValidationErrors = message.IsTrimmedNullOrEmpty() ? new() : new() { new ApiValidationError { Message = message } } });
    }

    public new static ApiValidationResult<T> Forbidden(IEnumerable<ApiValidationError> validationErrors)
    {
        return new ApiValidationResult<T>(ApiValidationResultStatus.Forbidden with { ValidationErrors = validationErrors.ToList() });
    }

    /// <summary></summary>
    public new static ApiValidationResult<T> Unavailable(string message = "")
    {
        return new ApiValidationResult<T>(ApiValidationResultStatus.Unavailable with { ValidationErrors = message.IsTrimmedNullOrEmpty() ? new() : new() { new ApiValidationError { Message = message } } });
    }

    public new static ApiValidationResult<T> Unavailable(IEnumerable<ApiValidationError> validationErrors)
    {
        return new ApiValidationResult<T>(ApiValidationResultStatus.Unavailable with { ValidationErrors = validationErrors.ToList() });
    }

    public new static ApiValidationResult<T> TooManyRequests(TimeSpan? retryAfter = null)
    {
        return new ApiValidationResult<T>(ApiValidationResultStatus.TooManyRequests with { RetryAfter = retryAfter });
    }
 
    public static implicit operator ApiValidationResult<T>(ApiValidationResultStatus status) => new ApiValidationResult<T>(status);
    
    public static implicit operator ApiValidationResult<T>((ApiValidationResultStatus status, List<ApiValidationError> errors) statusAndError)
    {
        return statusAndError.status switch
        {
            ApiValidationResultStatus.ForbiddenStatus => Forbidden(statusAndError.errors),
            ApiValidationResultStatus.InvalidStatus => Invalid(statusAndError.errors),
            ApiValidationResultStatus.NotFoundStatus => NotFound(statusAndError.errors),
            ApiValidationResultStatus.TooManyRequestsStatus status => TooManyRequests(status.RetryAfter),
            ApiValidationResultStatus.UnavailableStatus => Unavailable(statusAndError.errors),
            ApiValidationResultStatus.ValidStatus => throw new Exception(""),
            _ => throw new NotImplementedException("{status}")
        };
    }
    public static implicit operator ApiValidationResult<T>((ApiValidationResultStatus status, string message) statusAndMessage)
    {
        return statusAndMessage.status switch
        {
            ApiValidationResultStatus.ForbiddenStatus => Forbidden(statusAndMessage.message),
            ApiValidationResultStatus.InvalidStatus => Invalid(statusAndMessage.message),
            ApiValidationResultStatus.NotFoundStatus => NotFound(statusAndMessage.message),
            ApiValidationResultStatus.TooManyRequestsStatus status => TooManyRequests(status.RetryAfter),
            ApiValidationResultStatus.UnavailableStatus => Unavailable(statusAndMessage.message),
            ApiValidationResultStatus.ValidStatus => throw new Exception(""),
            _ => throw new NotImplementedException("")
        };
    }

    public void Deconstruct(out ApiValidationResultStatus status, out T? data)
    {
        status = this.Status;
        data = this.Data;
    }

    public void Deconstruct(out ApiValidationResultStatus status, out T? data, out List<ApiValidationError> apiValidationErrors)
    {
        status = this.Status;
        data = this.Data;
        apiValidationErrors = this.ValidationErrors;
    }
}


