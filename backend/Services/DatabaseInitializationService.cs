using Microsoft.EntityFrameworkCore;
using PredictLottoNZ.Data;

namespace PredictLottoNZ.Services;

public interface IDatabaseInitializationService
{
    Task InitializeAsync();
}

public class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly LottoDbContext _context;
    private readonly ILogger<DatabaseInitializationService> _logger;
    
    public DatabaseInitializationService(
        LottoDbContext context, 
        ILogger<DatabaseInitializationService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task InitializeAsync()
    {
        const int maxRetries = 5;
        var delay = TimeSpan.FromSeconds(1);
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Attempting database initialization (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
                
                // Check if database can be connected to
                await _context.Database.CanConnectAsync();
                
                // Apply any pending migrations
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Applying {Count} pending migrations: {Migrations}", 
                        pendingMigrations.Count(), 
                        string.Join(", ", pendingMigrations));
                    
                    await _context.Database.MigrateAsync();
                    _logger.LogInformation("Database migrations applied successfully");
                }
                else
                {
                    _logger.LogInformation("Database is up to date, no migrations needed");
                }
                
                // Verify database schema by checking if tables exist
                await VerifyDatabaseSchemaAsync();
                
                _logger.LogInformation("Database initialization completed successfully");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database initialization attempt {Attempt} failed: {Message}", attempt, ex.Message);
                
                if (attempt == maxRetries)
                {
                    _logger.LogError(ex, "Database initialization failed after {MaxRetries} attempts", maxRetries);
                    throw new InvalidOperationException($"Failed to initialize database after {maxRetries} attempts", ex);
                }
                
                // Exponential backoff with jitter
                var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
                var totalDelay = delay + jitter;
                
                _logger.LogInformation("Waiting {Delay}ms before retry attempt {NextAttempt}", 
                    totalDelay.TotalMilliseconds, attempt + 1);
                
                await Task.Delay(totalDelay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
            }
        }
    }
    
    private async Task VerifyDatabaseSchemaAsync()
    {
        try
        {
            // Simple verification by checking if we can query each table
            var lottoDrawCount = await _context.LottoDraws.CountAsync();
            var combinationCount = await _context.NumberCombinations.CountAsync();
            var predictionCount = await _context.Predictions.CountAsync();
            
            _logger.LogInformation("Database schema verification successful. Tables contain: " +
                "LottoDraws={LottoDraws}, NumberCombinations={NumberCombinations}, Predictions={Predictions}",
                lottoDrawCount, combinationCount, predictionCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database schema verification failed");
            throw new InvalidOperationException("Database schema verification failed", ex);
        }
    }
}