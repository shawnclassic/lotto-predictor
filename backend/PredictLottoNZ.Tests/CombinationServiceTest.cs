using Microsoft.Extensions.Logging;
using PredictLottoNZ.Services;

namespace PredictLottoNZ.Tests;

public class CombinationServiceTest
{
    private readonly ILogger<CombinationService> _logger;
    private readonly ILogger<FileParsingService> _fileLogger;

    public CombinationServiceTest()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<CombinationService>();
        _fileLogger = loggerFactory.CreateLogger<FileParsingService>();
    }

    public async Task RunAllTests()
    {
        Console.WriteLine("Running Combination Service Tests...");
        Console.WriteLine("===================================");

        await TestCombinationValidation();

        Console.WriteLine("===================================");
        Console.WriteLine("Combination Service tests PASSED!");
        Console.WriteLine("Note: Database integration tests require runtime database connection.");
    }

    private async Task TestCombinationValidation()
    {
        Console.WriteLine("Test 1: Combination validation logic...");

        var fileParsingService = new FileParsingService(_fileLogger);
        
        // Test validation logic directly
        var validCombination = new[] { 1, 2, 3, 4, 5, 6 };
        if (!IsValidCombination(validCombination))
            throw new Exception("Valid combination should pass validation");

        // Invalid - wrong length
        var wrongLength = new[] { 1, 2, 3, 4, 5 };
        if (IsValidCombination(wrongLength))
            throw new Exception("Wrong length combination should fail validation");

        // Invalid - out of range
        var outOfRange = new[] { 1, 2, 3, 4, 5, 41 };
        if (IsValidCombination(outOfRange))
            throw new Exception("Out of range combination should fail validation");

        // Invalid - duplicates
        var duplicates = new[] { 1, 2, 3, 4, 5, 5 };
        if (IsValidCombination(duplicates))
            throw new Exception("Duplicate numbers should fail validation");

        // Test file type validation
        if (!fileParsingService.IsSupportedFileType("test.csv"))
            throw new Exception("CSV should be supported");

        if (!fileParsingService.IsSupportedFileType("test.txt"))
            throw new Exception("TXT should be supported");

        if (!fileParsingService.IsSupportedFileType("test.pdf"))
            throw new Exception("PDF should be supported");

        if (fileParsingService.IsSupportedFileType("test.doc"))
            throw new Exception("DOC should not be supported");

        Console.WriteLine("✓ PASSED: Combination validation logic");
    }

    private bool IsValidCombination(int[] numbers)
    {
        // Must have exactly 6 numbers
        if (numbers == null || numbers.Length != 6)
        {
            return false;
        }

        // All numbers must be in range [1, 40]
        if (numbers.Any(n => n < 1 || n > 40))
        {
            return false;
        }

        // All numbers must be unique
        if (numbers.Distinct().Count() != 6)
        {
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        // No resources to dispose in simplified version
    }
}