using BenchmarkDotNet.Attributes;
using ValidatorService.Helpers;

namespace ValidatorService.Benchmarks;

public class LuhnAlgorithmBenchmarks
{
    private const string NumericString = "12345678901234567890"; // Test input
    private const string NonNumericString = "12345a67890";       // To test failure cases

    [Benchmark]
    public bool CharIsValid_All()
    {
        return LuhnAlgorithm.ValidateCheckDigit(NumericString);
    }

    [Benchmark]
    public bool CharIsInvalid_All_NonNumeric()
    {
        return LuhnAlgorithm.ValidateCheckDigit(NonNumericString);
    }
}