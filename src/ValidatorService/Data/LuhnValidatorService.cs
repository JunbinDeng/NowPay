namespace ValidatorService.Data;

/// <summary>
/// Provides validation for credit card numbers using the Luhn algorithm.
/// </summary>
public class LuhnValidatorService : IValidatorService
{
    /// <inheritdoc />
    /// <exception cref="NotImplementedException">Thrown if validation is not implemented.</exception>
    public bool ValidateCardNumber(string? cardNumber)
    {
        throw new NotImplementedException();
    }
}
