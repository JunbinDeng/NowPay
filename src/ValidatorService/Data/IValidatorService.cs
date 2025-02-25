namespace ValidatorService.Data;

/// <summary>
/// Defines a contract for validating payment card numbers.
/// </summary>
public interface IValidatorService
{
    /// <summary>
    /// Validates whether a given card number meets the required format and algorithmic checks.
    /// </summary>
    /// <param name="cardNumber">The credit card number to validate.</param>
    /// <returns><c>true</c> if the card number is valid; otherwise, <c>false</c>.</returns>
    bool ValidateCardNumber(string? cardNumber);
}
