using ValidatorService.Helpers;

namespace ValidatorService.UnitTests;

public class LuhnAlgorithmTests
{
    [Theory]
    [InlineData("", false)]                     // Empty string
    [InlineData("0", false)]                    // Single digit
    [InlineData("1", false)]                    // Single digit
    [InlineData("00", false)]                   // Two digits
    [InlineData("18", false)]                   // Two digits
    [InlineData("26", false)]                   // Two digits
    [InlineData("34", false)]                   // Two digits
    [InlineData("42", false)]                   // Two digits
    [InlineData("59", false)]                   // Two digits
    [InlineData("67", false)]                   // Two digits
    [InlineData("75", false)]                   // Two digits
    [InlineData("83", false)]                   // Two digits
    [InlineData("91", false)]                   // Two digits
    [InlineData("133", false)]                  // Three digits
    [InlineData("0000000000000000", true)]      // Valid all zroes
    [InlineData("7624810", false)]              // Valid digit calculates as zero
    [InlineData("4532015112830366", true)]      // Valid Visa card
    [InlineData("5555555555554444", true)]      // Valid MasterCard
    [InlineData("3056930009020004", true)]      // Valid Diners Club card
    [InlineData("4012888888881881", true)]      // Valid Visa test number
    [InlineData("1234567812345678", false)]     // Invalid card number
    [InlineData("9999999999999999", false)]     // Invalid number
    [InlineData("3056930090020004", true)]      // Diners Club test card number with two digit transposition 09 -> 90
    [InlineData("3056930000920004", true)]      // Diners Club test card number with two digit transposition 90 -> 09
    [InlineData("5555555225554444", true)]      // MasterCard test card number with two digit twin error 55 -> 22
    [InlineData("5555555225554774", true)]      // MasterCard test card number with two digit twin error 44 -> 77
    [InlineData("3533111111111113", true)]      // JCB test card number with two digit twin error 66 -> 33
    [InlineData("3566111111111113", true)]      // JCB test credit card number
    [InlineData("808401234567893", true)]       // NPI (National Provider Identifier) 
    [InlineData("490154203237518", true)]       // IMEI (International Mobile Equipment Identity)
    [InlineData("5558555555554444", false)]     // MasterCard test card number with single digit transcription error 5 -> 8
    [InlineData("5558555555554434", false)]     // MasterCard test card number with single digit transcription error 4 -> 3
    [InlineData("3059630009020004", false)]     // Diners Club test card number with two digit transposition error 69 -> 96 
    [InlineData("3056930009002004", false)]     // Diners Club test card number with two digit transposition error 20 -> 02
    [InlineData("5559955555554444", false)]     // MasterCard test card number with two digit twin error 55 -> 99
    [InlineData("3566111144111113", false)]     // JCB test card number with two digit twin error 11 -> 44
    [InlineData("123A780", false)]              // Invalid character
    [InlineData("123B781", false)]              // Invalid character
    [InlineData("123C782", false)]              // Invalid character
    [InlineData("123D783", false)]              // Invalid character          
    [InlineData("123E784", false)]              // Invalid character
    [InlineData("123F785", false)]              // Invalid character
    [InlineData("123G786", false)]              // Invalid character
    [InlineData("123H787", false)]              // Invalid character
    [InlineData("123I788", false)]              // Invalid character
    [InlineData("123J789", false)]              // Invalid character
    [InlineData("123-456-789", false)]          // Invalid character
    [InlineData("123$#^789%$&-())", false)]     // Invalid character
    [InlineData("12345678901234567890", false)] // Invalid 20 digits
    [InlineData("55555555555544445555", false)] // Invalid 20-digit MasterCard
    public void ValidateCardNumber_WhenCalled_ReturnsExpectedResult(string cardNumber, bool expected)
    {
        var result = LuhnAlgorithm.ValidateCheckDigit(cardNumber);
        Assert.Equal(expected, result);
    }
}
