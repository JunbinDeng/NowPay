namespace ValidatorService.Helpers;

/// <summary>
/// Provides utility methods for validating credit card numbers using the **Luhn Algorithm**.
/// </summary>
public static class LuhnAlgorithm
{
    private static readonly int[] _doubledValues = [0, 2, 4, 6, 8, 1, 3, 5, 7, 9];

    public static bool ValidateCheckDigit(string? str)
    {
        if (string.IsNullOrEmpty(str) || str.Length < 13 || str.Length > 19)
        {
            return false;
        }

        var sum = 0;
        var shouldApplyDouble = true;
        for (var index = str.Length - 2; index >= 0; index--)
        {
            var currentDigit = str[index] - '0';
            if (currentDigit < 0 || currentDigit > 9)
            {
                return false;
            }
            sum += shouldApplyDouble ? _doubledValues[currentDigit] : currentDigit;
            shouldApplyDouble = !shouldApplyDouble;
        }
        var checkDigit = (10 - (sum % 10)) % 10;

        return str[^1] - '0' == checkDigit;
    }
}
