using Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public interface IApiValidationResultStatusErrors
{
    List<ApiValidationError> ValidationErrors { get; }
}

[JsonConverter(typeof(JsonSubTypeConverter<ApiValidationResultStatus>))]
public abstract record ApiValidationResultStatus()
{
    public abstract string Type { get; }

    public record ValidStatus : ApiValidationResultStatus
    {
        public ValidStatus() : base() { }
        public ValidStatus(object? data) : base()
        {
            this.Data = data;
        }
        public override string Type => "Valid";
        public object? Data { get; init; }
        public virtual bool Equals(ValidStatus? other) => (object)this == other || ((object?)other != null && EqualityContract == other!.EqualityContract);
        public override int GetHashCode() => EqualityComparer<Type>.Default.GetHashCode(EqualityContract);
    }

    public record NotFoundStatus : ApiValidationResultStatus, IApiValidationResultStatusErrors
    {
        public NotFoundStatus() : base() { }
        public NotFoundStatus(List<ApiValidationError> validationErrors) : base()
        {
            this.ValidationErrors = validationErrors;
        }

        public List<ApiValidationError> ValidationErrors { get; init; } = new();
        public override string Type => "NotFound";
        public virtual bool Equals(NotFoundStatus? other) => (object)this == other || ((object?)other != null && EqualityContract == other!.EqualityContract);
        public override int GetHashCode() => EqualityComparer<Type>.Default.GetHashCode(EqualityContract);
        public override string ToString()
        {
            var jObject = JObject.FromObject(this);
            jObject["Status"] = this.GetType().Name;
            return jObject.ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }
    public record InvalidStatus : ApiValidationResultStatus, IApiValidationResultStatusErrors
    {
        public InvalidStatus() : base() { }
        public InvalidStatus(List<ApiValidationError> validationErrors) : base()
        {
            this.ValidationErrors = validationErrors;
        }

        public List<ApiValidationError> ValidationErrors { get; init; } = new();
        public override string Type => "Invalid";
        public virtual bool Equals(InvalidStatus?other) => (object)this == other || ((object?)other != null && EqualityContract == other!.EqualityContract);
        public override int GetHashCode() => EqualityComparer<Type>.Default.GetHashCode(EqualityContract);
        public override string ToString()
        {
            var jObject = JObject.FromObject(this);
            jObject["Status"] = this.GetType().Name;
            return jObject.ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }
    public record ForbiddenStatus : ApiValidationResultStatus, IApiValidationResultStatusErrors
    {
        public ForbiddenStatus() : base() { }
        public ForbiddenStatus(List<ApiValidationError> validationErrors) : base()
        {
            this.ValidationErrors = validationErrors;
        }
        public List<ApiValidationError> ValidationErrors { get; init; } = new();
        public override string Type => "Forbidden";
        public virtual bool Equals(ForbiddenStatus? other) => (object)this == other || ((object?)other != null && EqualityContract == other!.EqualityContract);
        public override int GetHashCode() => EqualityComparer<Type>.Default.GetHashCode(EqualityContract);
        public override string ToString()
        {
            var jObject = JObject.FromObject(this);
            jObject["Status"] = this.GetType().Name;
            return jObject.ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }
    public record UnavailableStatus : ApiValidationResultStatus, IApiValidationResultStatusErrors
    {
        public UnavailableStatus() : base() { }
        public UnavailableStatus(List<ApiValidationError> validationErrors) : base()
        {
            this.ValidationErrors = validationErrors;
        }
        public List<ApiValidationError> ValidationErrors { get; init; } = new();
        public override string Type => "Unavailable";
        public virtual bool Equals(UnavailableStatus? other) => (object)this == other || ((object?)other != null && EqualityContract == other!.EqualityContract);
        public override int GetHashCode() => EqualityComparer<Type>.Default.GetHashCode(EqualityContract);
        public override string ToString()
        {
            var jObject = JObject.FromObject(this);
            jObject["Status"] = this.GetType().Name;
            return jObject.ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }
    public record TooManyRequestsStatus : ApiValidationResultStatus
    {
        public TooManyRequestsStatus() : base() { }
        public TooManyRequestsStatus(TimeSpan? retryAfter) : base()
        {
            this.RetryAfter = retryAfter;
        }
        public TimeSpan? RetryAfter { get; init; }
        public override string Type => "TooManyRequests";
        public virtual bool Equals(TooManyRequestsStatus? other) => (object)this == other || ((object?)other != null && EqualityContract == other!.EqualityContract);
        public override int GetHashCode() => EqualityComparer<Type>.Default.GetHashCode(EqualityContract);
    }

    public static ValidStatus Valid => new();
    public static NotFoundStatus NotFound => new (new());
    public static InvalidStatus Invalid => new(new());
    public static ForbiddenStatus Forbidden => new(new());
    public static UnavailableStatus Unavailable => new(new());
    public static TooManyRequestsStatus TooManyRequests => new TooManyRequestsStatus(null);
    public static IEnumerable<ApiValidationResultStatus> GetAllStatuses()
    {
        yield return Valid;
        yield return NotFound;
        yield return Invalid;
        yield return Forbidden;
        yield return Unavailable;
        yield return TooManyRequests;
    }

    public static ValidStatus ValidWith<T>(T? data) => new ValidStatus(data);
    public static NotFoundStatus NotFoundWith(string message) => new(ToApiValidationErrorList(message));
    public static NotFoundStatus NotFoundWith(IEnumerable<ApiValidationError> errors) => new(errors.ToList());
    public static InvalidStatus InvalidWith(string message) => new(ToApiValidationErrorList(message));
    public static InvalidStatus InvalidWith(string message, string propertyName) => new(ToApiValidationErrorList(message, propertyName));
    public static InvalidStatus InvalidWith(IEnumerable<ApiValidationError> errors) => new(errors.ToList());
    public static ForbiddenStatus ForbiddenWith(string message) => new(ToApiValidationErrorList(message));
    public static ForbiddenStatus ForbiddenWith(IEnumerable<ApiValidationError> errors) => new(errors.ToList());
    public static UnavailableStatus UnavailableWith(string message) => new(ToApiValidationErrorList(message));
    public static UnavailableStatus UnavailableWith(IEnumerable<ApiValidationError> errors) => new(errors.ToList());
    public static UnavailableStatus UnavailableWith(Exception e) => new(new List<ApiValidationError> { new ApiValidationError { Message = e.Message, Data = e.Format() } });
    public static TooManyRequestsStatus TooManyRequestsWith(TimeSpan? retryAfter) => new(retryAfter);

    private static List<ApiValidationError> ToApiValidationErrorList(string message, string propertyName = "") => message.IsTrimmedNullOrEmpty() ? new() : new() { new() { Message = message, PropertyName = propertyName } };
}