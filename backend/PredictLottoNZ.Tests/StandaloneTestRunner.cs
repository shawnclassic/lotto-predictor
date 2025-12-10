using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PredictLottoNZ.Tests;

/// <summary>
/// Standalone test runner that can execute property-based tests without dependencies
/// </summary>
public static class StandaloneTestRunner
{
    public static async Task RunTests()
    {
        Console.WriteLine("Starting Standalone Property-Based Tests...");
        Console.WriteLine("==========================================");

        try
        {
            // Temporarily disable database test due to InMemory package issues
            // await DatabaseInitializationPropertyTest.RunDatabaseInitializationPropertyTest();
            await SimpleCsvParsingTest.RunSimpleCsvParsingTest();
            await DuplicateDetectionPropertyTest.RunDuplicateDetectionTest();
            await ErrorHandlingPropertyTest.RunErrorHandlingTest();
            await LottoQueryServiceTest.RunLottoQueryServiceTests();
            await DrawExistencePropertyTest.RunDrawExistenceTest();
            await LatestDrawRetrievalPropertyTest.RunLatestDrawRetrievalTest();
            
            // New combination processing tests
            var fileParsingTest = new FileParsingServiceTest();
            await fileParsingTest.RunAllTests();
            
            await FileFormatParsingPropertyTest.RunFileFormatParsingTest();
            
            var combinationTest = new CombinationServiceTest();
            await combinationTest.RunAllTests();
            combinationTest.Dispose();
            
            await CombinationValidationPropertyTest.RunCombinationValidationTest();
            await CombinationStoragePropertyTest.RunCombinationStorageTest();
            
            // Frequency calculation tests
            await FrequencyCalculationPropertyTest.RunFrequencyCalculationTest();
            
            // Provider fallback tests
            await ProviderFallbackPropertyTest.RunProviderFallbackPropertyTest();
            
            // Prediction persistence tests
            await PredictionPersistencePropertyTest.RunPredictionPersistencePropertyTest();
            
            // Configuration management tests
            await ConfigurationManagementPropertyTest.RunConfigurationManagementTest();
            
            Console.WriteLine("\n==========================================");
            Console.WriteLine("All standalone tests completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ TESTS FAILED: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}