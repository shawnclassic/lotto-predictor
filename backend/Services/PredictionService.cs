using Microsoft.EntityFrameworkCore;
using PredictLottoNZ.Data;
using PredictLottoNZ.Models;
using PredictLottoNZ.Models.DTOs;
using System.Text.Json;

namespace PredictLottoNZ.Services;

public interface IPredictionService
{
    Task<IEnumerable<PredictionResult>> GeneratePredictionsAsync(int count);
    Task<IEnumerable<PredictionResult>> GetStoredPredictionsAsync(int count);
    Task StorePredictionsAsync(IEnumerable<PredictionResult> predictions);
}

public class CircuitBreakerState
{
    public bool IsOpen { get; set; }
    public DateTime LastFailureTime { get; set; }
    public int FailureCount { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public int FailureThreshold { get; set; } = 3;
}

public class PredictionService : IPredictionService
{
    private readonly IEnumerable<IPredictionProvider> _providers;
    private readonly LottoDbContext _context;
    private readonly ILogger<PredictionService> _logger;
    private readonly Dictionary<string, CircuitBreakerState> _circuitBreakers;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public PredictionService(
        IEnumerable<IPredictionProvider> providers,
        LottoDbContext context,
        ILogger<PredictionService> logger)
    {
        _providers = providers.OrderBy(p => p.Priority); // Order by priority (lower = higher priority)
        _context = context;
        _logger = logger;
        _circuitBreakers = new Dictionary<string, CircuitBreakerState>();
        
        // Initialize circuit breakers for each provider
        foreach (var provider in _providers)
        {
            _circuitBreakers[provider.ProviderName] = new CircuitBreakerState();
        }
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }
    
    public async Task<IEnumerable<PredictionResult>> GeneratePredictionsAsync(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be positive", nameof(count));
            
        _logger.LogInformation("Generating {Count} predictions using provider chain", count);
        
        // Try providers in priority order
        foreach (var provider in _providers)
        {
            var circuitBreaker = _circuitBreakers[provider.ProviderName];
            
            // Check circuit breaker state
            if (IsCircuitBreakerOpen(circuitBreaker))
            {
                _logger.LogWarning("Circuit breaker is open for provider {ProviderName}, skipping", provider.ProviderName);
                continue;
            }
            
            try
            {
                _logger.LogInformation("Attempting to generate predictions using provider: {ProviderName}", provider.ProviderName);
                
                var predictions = await provider.PredictAsync(count);
                var predictionList = predictions.ToList();
                
                if (predictionList.Any())
                {
                    _logger.LogInformation("Successfully generated {Count} predictions using provider: {ProviderName}", 
                        predictionList.Count, provider.ProviderName);
                    
                    // Reset circuit breaker on success
                    ResetCircuitBreaker(circuitBreaker);
                    
                    // Store predictions with comprehensive logging for training data
                    await StorePredictionsWithPayloadAsync(predictionList, provider.ProviderName, count);
                    
                    return predictionList;
                }
                
                _logger.LogWarning("Provider {ProviderName} returned no predictions", provider.ProviderName);
                RecordProviderFailure(circuitBreaker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Provider {ProviderName} failed to generate predictions", provider.ProviderName);
                RecordProviderFailure(circuitBreaker);
                // Continue to next provider
            }
        }
        
        _logger.LogError("All prediction providers failed to generate predictions");
        throw new InvalidOperationException("No prediction providers were able to generate predictions");
    }
    
    public async Task<IEnumerable<PredictionResult>> GetStoredPredictionsAsync(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be positive", nameof(count));
            
        _logger.LogInformation("Retrieving {Count} stored predictions", count);
        
        var predictions = await _context.Predictions
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
            
        return predictions.Select(PredictionResult.FromEntity);
    }
    
    public async Task StorePredictionsAsync(IEnumerable<PredictionResult> predictions)
    {
        await StorePredictionsWithPayloadAsync(predictions, "Unknown", 0);
    }
    
    private async Task StorePredictionsWithPayloadAsync(IEnumerable<PredictionResult> predictions, string providerName, int requestedCount)
    {
        var predictionList = predictions.ToList();
        
        if (!predictionList.Any())
        {
            _logger.LogWarning("No predictions to store");
            return;
        }
        
        _logger.LogInformation("Storing {Count} predictions from provider {ProviderName}", predictionList.Count, providerName);
        
        // Create request payload for training data
        var requestPayload = new
        {
            provider = providerName,
            requestedCount = requestedCount,
            timestamp = DateTime.UtcNow,
            requestId = Guid.NewGuid()
        };
        
        // Create response payload for training data
        var responsePayload = new
        {
            provider = providerName,
            predictionsGenerated = predictionList.Count,
            predictions = predictionList.Select(p => new
            {
                numbers = p.Numbers,
                score = p.Score,
                source = p.Source,
                createdAt = p.CreatedAt
            }),
            timestamp = DateTime.UtcNow
        };
        
        var requestJson = JsonSerializer.Serialize(requestPayload, _jsonOptions);
        var responseJson = JsonSerializer.Serialize(responsePayload, _jsonOptions);
        
        var entities = predictionList.Select(p => new Prediction
        {
            CreatedAt = p.CreatedAt,
            Source = p.Source,
            Score = p.Score,
            Number1 = p.Numbers[0],
            Number2 = p.Numbers[1],
            Number3 = p.Numbers[2],
            Number4 = p.Numbers[3],
            Number5 = p.Numbers[4],
            Number6 = p.Numbers[5],
            RawRequestPayload = requestJson,
            RawResponsePayload = responseJson
        }).ToList();
        
        await _context.Predictions.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Successfully stored {Count} predictions with training data payloads", entities.Count);
    }
    
    private bool IsCircuitBreakerOpen(CircuitBreakerState circuitBreaker)
    {
        if (!circuitBreaker.IsOpen)
            return false;
            
        // Check if timeout has elapsed
        if (DateTime.UtcNow - circuitBreaker.LastFailureTime > circuitBreaker.Timeout)
        {
            _logger.LogInformation("Circuit breaker timeout elapsed, attempting to close");
            circuitBreaker.IsOpen = false;
            circuitBreaker.FailureCount = 0;
            return false;
        }
        
        return true;
    }
    
    private void RecordProviderFailure(CircuitBreakerState circuitBreaker)
    {
        circuitBreaker.FailureCount++;
        circuitBreaker.LastFailureTime = DateTime.UtcNow;
        
        if (circuitBreaker.FailureCount >= circuitBreaker.FailureThreshold)
        {
            circuitBreaker.IsOpen = true;
            _logger.LogWarning("Circuit breaker opened due to {FailureCount} consecutive failures", 
                circuitBreaker.FailureCount);
        }
    }
    
    private void ResetCircuitBreaker(CircuitBreakerState circuitBreaker)
    {
        if (circuitBreaker.FailureCount > 0 || circuitBreaker.IsOpen)
        {
            _logger.LogInformation("Resetting circuit breaker after successful operation");
            circuitBreaker.FailureCount = 0;
            circuitBreaker.IsOpen = false;
        }
    }
}