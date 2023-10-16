/// <summary></summary>
public class ApiValidationError
{
    /// <summary>
    /// (Optional) Indicates the object property that contains the error
    /// </summary>
    public string? PropertyName { get; set; }
    /// <summary>
    /// Validation error message
    /// </summary>
    public string Message { get; set; } = "";
    /// <summary>
    /// 
    /// </summary>
    public string? Code { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public object? Data { get; set; }
}

