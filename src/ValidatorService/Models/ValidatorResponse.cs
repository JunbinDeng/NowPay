namespace ValidatorService.Models;

/// <summary>
/// Represents a response to validate a credit card number.
/// </summary>
public class ValidatorResponse
{
    /// <summary>
    /// Indicates whether the provided credit card number is valid.
    /// </summary>
    public bool IsValid { get; set; }
}
