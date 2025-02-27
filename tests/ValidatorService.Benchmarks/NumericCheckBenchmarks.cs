using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

namespace ValidatorService.Benchmarks;

public class NumericCheckBenchmarks
{
    private const string NumericString = "12345678901234567890"; // Test input
    private const string NonNumericString = "12345a67890";       // To test failure cases

    protected virtual Regex NumericRegex => new Regex("^[0-9]+$", RegexOptions.Compiled);

    [Benchmark]
    public bool CharIsDigit_All()
    {
        return NumericString.All(char.IsDigit);
    }

    [Benchmark]
    public bool RegexIsMatch()
    {
        return NumericRegex.IsMatch(NumericString);
    }

    [Benchmark]
    public bool CharIsDigit_All_NonNumeric()
    {
        return NonNumericString.All(char.IsDigit);
    }

    [Benchmark]
    public bool RegexIsMatch_NonNumeric()
    {
        return NumericRegex.IsMatch(NonNumericString);
    }
}
