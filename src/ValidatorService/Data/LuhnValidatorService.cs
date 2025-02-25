using ValidatorService.Helpers;

namespace ValidatorService.Data;

/// <summary>
/// Provides validation for credit card numbers using the Luhn algorithm.
/// </summary>
public class LuhnValidatorService : IValidatorService
{
    /// <summary>
    /// Validates a credit card number using the Luhn algorithm.
    /// </summary>
    /// <param name="cardNumber">The credit card number to validate.</param>
    /// <returns>
    /// Returns <c>true</c> if the card number is valid; otherwise, <c>false</c>.
    /// </returns>
    public bool ValidateCardNumber(string? cardNumber)
    {
        return LuhnAlgorithm.ValidateCheckDigit(cardNumber);
    }
}
