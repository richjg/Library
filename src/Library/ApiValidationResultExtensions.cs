using Library;

public static class ApiValidationResultExtensions
{
    public static ApiValidationResult ToNotFoundApiValidatioResult(this IEnumerable<ApiValidationError> apiValidationErrors) => ApiValidationResult.NotFound(apiValidationErrors);
    public static ApiValidationResult ToInvalidApiValidatioResult(this IEnumerable<ApiValidationError> apiValidationErrors) => ApiValidationResult.Invalid(apiValidationErrors);
    public static ApiValidationResult<T> ToInvalidApiValidatioResult<T>(this IEnumerable<ApiValidationError> apiValidationErrors) => ApiValidationResult<T>.Invalid(apiValidationErrors);
    public static ApiValidationResult<T> ToValidResult<T>(this T t) => ApiValidationResult.Valid(t);
    public static Task<ApiValidationResult<T>> ToValidResultAsync<T>(this T t) => ApiValidationResult.Valid(t).AsCompletedTask();
    public static async Task<ApiValidationResult<T>> ToValidResultAsync<T>(this Task<T> t) => (await t).ToValidResult();
    public static ApiValidationResult<Tout> ConvertNonValidResult<Tout>(this ApiValidationResult apiValidationResult) => new ApiValidationResult<Tout>(apiValidationResult.Status);

    public static string ConcatMessages(this IEnumerable<ApiValidationError> apiValidationErrors) => apiValidationErrors.Select(e => e.Message).Concat(" ");
    public static async Task<T?> GetDataOrThrow<T>(this Task<ApiValidationResult<T>> apiValidationResult) => (await apiValidationResult).GetDataOrThrow();
    public static T? GetDataOrThrow<T>(this ApiValidationResult<T> apiValidationResult)
    {
        if (apiValidationResult.HasErrors)
        {
            throw new Exception(apiValidationResult.ValidationErrors.ConcatMessages());
        }

        return apiValidationResult.Data;
    }

    public static List<ApiValidationError> Add(this List<ApiValidationError> apiValidationErrors, string message, string propertyName = "", string code = "")
    {
        apiValidationErrors.Add(new ApiValidationError { Message = message, PropertyName = propertyName, Code = code });
        return apiValidationErrors;
    }

    public static ApiValidationResult<T> ToValidationResult<T>(this ApiValidationResultStatus apiValidationResultStatus) => new ApiValidationResult<T>(apiValidationResultStatus);
    public static Task<ApiValidationResult<T>> ToValidationResultAsync<T>(this ApiValidationResultStatus apiValidationResultStatus) => new ApiValidationResult<T>(apiValidationResultStatus).AsCompletedTask();
}

