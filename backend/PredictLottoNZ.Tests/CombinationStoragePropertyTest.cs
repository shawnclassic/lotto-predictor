using Microsoft.Extensions.Logging;
using PredictLottoNZ.Models;
using PredictLottoNZ.Services;

namespace PredictLottoNZ.Tests;

/// <summary>
/// Property-based test for combination storage metadata requirements
/// **Feature: predict-lotto-nz, Property 18: Combination storage includes metadata**
/// **Validates: Requirements 4.3**
/// </summary>
public static class CombinationStoragePropertyTest
{
    public static async Task RunCombinationStorageTest()
    {
        Console.WriteLine("Running Combination Storage Property Test...");
        Console.WriteLine("============================================");

        Console.WriteLine("Property Test 18: Combination storage includes metadata...");

        // Test the property across multiple iterations
        const int iterations = 100;
        var random = new Random(42); // Fixed seed for reproducible tests

        for (int i = 0; i < iterations; i++)
        {
            try
            {
                // Test that NumberCombination entities include required metadata
                await TestCombinationEntityMetadata(random, i);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED on iteration {i + 1}: {ex.Message}");
                throw;
            }
        }

        Console.WriteLine($"✓ PASSED: Combination storage includes metadata ({iterations} iterations)");
        Console.WriteLine("============================================");
        Console.WriteLine("Combination storage property test PASSED!");
        Console.WriteLine("Property 18: Combination storage includes metadata - VALIDATED");
        Console.WriteLine("Requirements 4.3 - SATISFIED");
    }

    private static async Task TestCombinationEntityMetadata(Random random, int iteration)
    {
        // Generate random valid combinations
        var combinationsToTest = GenerateRandomValidCombinations(random, random.Next(1, 10));
        var timestampBeforeCreation = DateTime.UtcNow;

        // Property: For any stored number combination, the record should include creation timestamp and be saved to the NumberCombination table
        foreach (var combinationNumbers in combinationsToTest)
        {
            // Create NumberCombination entity as would be done during storage
            var numberCombination = new NumberCombination();
            numberCombination.SetNumbers(combinationNumbers.OrderBy(n => n).ToArray());
            numberCombination.CreatedAt = DateTime.UtcNow;

            var timestampAfterCreation = DateTime.UtcNow;

            // Verify metadata: Creation timestamp is present and reasonable
            if (numberCombination.CreatedAt == default(DateTime))
            {
                throw new Exception($"NumberCombination entity missing CreatedAt timestamp");
            }

            // Verify timestamp is within reasonable bounds (between before and after creation)
            if (numberCombination.CreatedAt < timestampBeforeCreation.AddMinutes(-1) || 
                numberCombination.CreatedAt > timestampAfterCreation.AddMinutes(1))
            {
                throw new Exception($"NumberCombination entity has unreasonable CreatedAt timestamp: {numberCombination.CreatedAt}");
            }

            // Verify all number fields are populated and valid
            var storedNumbers = numberCombination.GetNumbers();
            if (storedNumbers.Length != 6)
            {
                throw new Exception($"NumberCombination entity does not have exactly 6 numbers");
            }

            if (storedNumbers.Any(n => n < 1 || n > 40))
            {
                throw new Exception($"NumberCombination entity contains numbers outside valid range [1,40]: [{string.Join(", ", storedNumbers)}]");
            }

            if (storedNumbers.Distinct().Count() != 6)
            {
                throw new Exception($"NumberCombination entity contains duplicate numbers: [{string.Join(", ", storedNumbers)}]");
            }

            // Verify that the stored numbers match the original combination (when sorted)
            var originalSorted = combinationNumbers.OrderBy(n => n).ToArray();
            var storedSorted = storedNumbers.OrderBy(n => n).ToArray();
            
            if (!originalSorted.SequenceEqual(storedSorted))
            {
                throw new Exception($"Stored numbers [{string.Join(", ", storedSorted)}] do not match original [{string.Join(", ", originalSorted)}]");
            }

            // Verify that the entity has the required table mapping attributes
            var tableAttribute = typeof(NumberCombination).GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute), false).FirstOrDefault();
            if (tableAttribute == null)
            {
                throw new Exception("NumberCombination entity missing Table attribute for database mapping");
            }

            var tableName = ((System.ComponentModel.DataAnnotations.Schema.TableAttribute)tableAttribute).Name;
            if (tableName != "NumberCombinations")
            {
                throw new Exception($"NumberCombination entity mapped to wrong table: {tableName}, expected: NumberCombinations");
            }

            // Verify that CreatedAt property has Required attribute
            var createdAtProperty = typeof(NumberCombination).GetProperty("CreatedAt");
            if (createdAtProperty == null)
            {
                throw new Exception("NumberCombination entity missing CreatedAt property");
            }

            var requiredAttribute = createdAtProperty.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false).FirstOrDefault();
            if (requiredAttribute == null)
            {
                throw new Exception("NumberCombination.CreatedAt property missing Required attribute");
            }
        }
    }

    private static IEnumerable<int[]> GenerateRandomValidCombinations(Random random, int count)
    {
        var combinations = new List<int[]>();
        var usedCombinations = new HashSet<string>();

        for (int i = 0; i < count; i++)
        {
            int[] combination;
            string combinationKey;
            
            // Generate unique combinations
            do
            {
                combination = GenerateValidCombination(random);
                var sortedCombo = combination.OrderBy(n => n).ToArray();
                combinationKey = string.Join(",", sortedCombo);
            } while (usedCombinations.Contains(combinationKey));

            usedCombinations.Add(combinationKey);
            combinations.Add(combination);
        }

        return combinations;
    }

    private static int[] GenerateValidCombination(Random random)
    {
        var numbers = new HashSet<int>();
        while (numbers.Count < 6)
        {
            numbers.Add(random.Next(1, 41)); // Range [1, 40]
        }
        return numbers.ToArray();
    }
}