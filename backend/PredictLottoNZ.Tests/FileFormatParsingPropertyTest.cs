using Microsoft.Extensions.Logging;
using PredictLottoNZ.Services;
using System.Text;

namespace PredictLottoNZ.Tests;

/// <summary>
/// Property-based test for file format parsing universality
/// **Feature: predict-lotto-nz, Property 7: File format parsing is universal**
/// **Validates: Requirements 4.1**
/// </summary>
public static class FileFormatParsingPropertyTest
{
    public static async Task RunFileFormatParsingTest()
    {
        Console.WriteLine("Running File Format Parsing Property Test...");
        Console.WriteLine("============================================");

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<FileParsingService>();
        var fileParsingService = new FileParsingService(logger);

        Console.WriteLine("Property Test 7: File format parsing is universal...");

        // Test the property across multiple iterations
        const int iterations = 100;
        var random = new Random(42); // Fixed seed for reproducible tests

        for (int i = 0; i < iterations; i++)
        {
            try
            {
                // Generate random valid combinations
                var combinations = GenerateRandomCombinations(random, random.Next(1, 6));
                
                // Test CSV format
                await TestCsvFormat(fileParsingService, combinations);
                
                // Test TXT format (line-based)
                await TestTxtFormat(fileParsingService, combinations);
                
                // Test TXT format (comma-separated)
                await TestTxtCommaSeparatedFormat(fileParsingService, combinations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED on iteration {i + 1}: {ex.Message}");
                throw;
            }
        }

        Console.WriteLine($"✓ PASSED: File format parsing is universal ({iterations} iterations)");
        Console.WriteLine("============================================");
        Console.WriteLine("File format parsing property test PASSED!");
        Console.WriteLine("Property 7: File format parsing is universal - VALIDATED");
        Console.WriteLine("Requirements 4.1 - SATISFIED");
    }

    private static List<int[]> GenerateRandomCombinations(Random random, int count)
    {
        var combinations = new List<int[]>();
        
        for (int i = 0; i < count; i++)
        {
            var numbers = new HashSet<int>();
            while (numbers.Count < 6)
            {
                numbers.Add(random.Next(1, 41)); // Range [1, 40]
            }
            combinations.Add(numbers.OrderBy(n => n).ToArray());
        }
        
        return combinations;
    }

    private static async Task TestCsvFormat(IFileParsingService service, List<int[]> expectedCombinations)
    {
        // Create CSV content
        var csvContent = string.Join("\n", expectedCombinations.Select(combo => string.Join(",", combo)));
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var parsedCombinations = await service.ParseFileAsync(stream, "test.csv");
        
        ValidateParsedCombinations(expectedCombinations, parsedCombinations.ToList(), "CSV");
    }

    private static async Task TestTxtFormat(IFileParsingService service, List<int[]> expectedCombinations)
    {
        // Create TXT content (line-based: one number per line, 6 consecutive lines per combination)
        var txtLines = new List<string>();
        foreach (var combo in expectedCombinations)
        {
            txtLines.AddRange(combo.Select(n => n.ToString()));
        }
        var txtContent = string.Join("\n", txtLines);
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(txtContent));
        var parsedCombinations = await service.ParseFileAsync(stream, "test.txt");
        
        ValidateParsedCombinations(expectedCombinations, parsedCombinations.ToList(), "TXT line-based");
    }

    private static async Task TestTxtCommaSeparatedFormat(IFileParsingService service, List<int[]> expectedCombinations)
    {
        // Create TXT content (comma-separated, one combination per line)
        var txtContent = string.Join("\n", expectedCombinations.Select(combo => string.Join(",", combo)));
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(txtContent));
        var parsedCombinations = await service.ParseFileAsync(stream, "test.txt");
        
        ValidateParsedCombinations(expectedCombinations, parsedCombinations.ToList(), "TXT comma-separated");
    }

    private static void ValidateParsedCombinations(List<int[]> expected, List<int[]> actual, string format)
    {
        if (actual.Count != expected.Count)
        {
            throw new Exception($"{format}: Expected {expected.Count} combinations, got {actual.Count}");
        }

        for (int i = 0; i < expected.Count; i++)
        {
            var expectedCombo = expected[i].OrderBy(n => n).ToArray();
            var actualCombo = actual[i].OrderBy(n => n).ToArray();
            
            if (!expectedCombo.SequenceEqual(actualCombo))
            {
                throw new Exception($"{format}: Combination {i} mismatch. Expected: [{string.Join(", ", expectedCombo)}], Got: [{string.Join(", ", actualCombo)}]");
            }
            
            // Validate each combination meets the constraints
            if (!IsValidCombination(actualCombo))
            {
                throw new Exception($"{format}: Invalid combination parsed: [{string.Join(", ", actualCombo)}]");
            }
        }
    }

    private static bool IsValidCombination(int[] numbers)
    {
        // Must have exactly 6 numbers
        if (numbers.Length != 6)
            return false;
            
        // All numbers must be in range [1, 40]
        if (numbers.Any(n => n < 1 || n > 40))
            return false;
            
        // All numbers must be unique
        return numbers.Distinct().Count() == 6;
    }
}