using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PredictLottoNZ.Data;
using PredictLottoNZ.Models;
using PredictLottoNZ.Models.DTOs;
using PredictLottoNZ.Services;
using System.Text.Json;

namespace PredictLottoNZ.Tests;

/**
 * Feature: predict-lotto-nz, Property 17: Data preservation maintains historical integrity
 */
public static class DataPreservationPropertyTest
{
    public static async Task RunDataPreservationPropertyTest()
    {
        Console.WriteLine("Running Data Preservation Property Test...");
        Console.WriteLine("==========================================");

        try
        {
            Console.WriteLine("Property Test 17: Data preservation maintains historical integrity...");
            await DataPreservationMaintainsHistoricalIntegrity_PropertyTest();
            Console.WriteLine("✓ PASSED: Data preservation maintains historical integrity (100 iterations)");
            
            Console.WriteLine("\n==========================================");
            Console.WriteLine("Data preservation property test PASSED!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ FAILED: Data preservation property test failed: {ex.Message}");
            throw;
        }
    }

    private static async Task DataPreservationMaintainsHistoricalIntegrity_PropertyTest()
    {
        var random = new Random(42);
        
        for (int iteration = 0; iteration < 100; iteration++)
        {
            using var serviceProvider = CreateServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LottoDbContext>();
            var trainingDataService = scope.ServiceProvider.GetRequiredService<ITrainingDataService>();
            
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            
            var originalTimestamp = DateTime.UtcNow.AddDays(-random.Next(1, 365));
            
            // Create test data
            var originalDraw = new LottoDraw
            {
                Draw = random.Next(1, 10000),
                Date = originalTimestamp.Date,
                CreatedAt = originalTimestamp,
                WinningNumber1 = random.Next(1, 41),
                WinningNumber2 = random.Next(1, 41),
                WinningNumber3 = random.Next(1, 41),
                WinningNumber4 = random.Next(1, 41),
                WinningNumber5 = random.Next(1, 41),
                WinningNumber6 = random.Next(1, 41),
                BonusNumber = random.Next(1, 41),
                Powerball = random.Next(1, 11),
                Division1Prize = (decimal)(random.NextDouble() * 1000000),
                FromLast = $"Test-{iteration}"
            };
            
            await context.LottoDraws.AddAsync(originalDraw);
            await context.SaveChangesAsync();
            
            // Export and verify preservation
            var export = await trainingDataService.ExportTrainingDataAsync();
            
            var exportedDraw = export.LottoDraws.FirstOrDefault(d => d.Draw == originalDraw.Draw);
            if (exportedDraw == null)
                throw new Exception($"Lottery draw {originalDraw.Draw} was not preserved in export");
            
            if (exportedDraw.Date != originalDraw.Date)
                throw new Exception($"Draw date not preserved: expected {originalDraw.Date}, got {exportedDraw.Date}");
            
            if (exportedDraw.CreatedAt != originalDraw.CreatedAt)
                throw new Exception($"Draw creation timestamp not preserved");
        }
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddDbContext<LottoDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
        services.AddScoped<ITrainingDataService, TrainingDataService>();
        return services.BuildServiceProvider();
    }
}
