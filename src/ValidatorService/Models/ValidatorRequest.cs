namespace ValidatorService.Models;

/// <summary>
/// Represents a request to validate a credit card number.
/// </summary>
public class ValidatorRequest
{
    /// <summary>
    /// Gets or sets the credit card number to validate.
    /// </summary>
    /// <example>4111111111111111</example>
    public string? CardNumber { get; set; }
}
