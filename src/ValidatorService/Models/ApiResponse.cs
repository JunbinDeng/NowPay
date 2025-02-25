namespace ValidatorService.Models;

/// <summary>
/// Standard API response wrapper that encapsulates the response status, message, optional data, and error details.
/// </summary>
/// <typeparam name="T">Type of the data payload.</typeparam>
/// <param name="status">HTTP status code of the response.</param>
/// <param name="message">Descriptive message about the response.</param>
/// <param name="data">The response data (optional, null if not applicable).</param>
/// <param name="error">Details of an error if applicable (null if no error).</param>
public class ApiResponse<T>(int status, string message, T? data = default, ErrorDetails? error = null)
{
    /// <summary>
    /// HTTP status code of the response (e.g., 200, 400, 422).
    /// </summary>
    public int Status { get; set; } = status;

    /// <summary>
    /// A descriptive message explaining the response status.
    /// </summary>
    public string Message { get; set; } = message;

    /// <summary>
    /// The actual data returned in the response, if applicable.
    /// </summary>
    public T? Data { get; set; } = data;

    /// <summary>
    /// Error details, if an error occurred (null if the request was successful).
    /// </summary>
    public ErrorDetails? Error { get; set; } = error;
}

/// <summary>
/// Represents detailed information about an error that occurred during API processing.
/// </summary>
/// <param name="code">A unique identifier for the error type.</param>
/// <param name="details">A human-readable description of the error.</param>
public class ErrorDetails(string code, string details)
{
    /// <summary>
    /// A unique identifier for the error (e.g., "invalid_format", "missing_field").
    /// </summary>
    public string Code { get; set; } = code;

    /// <summary>
    /// A detailed description of the error, providing context for debugging.
    /// </summary>
    public string Details { get; set; } = details;
}
