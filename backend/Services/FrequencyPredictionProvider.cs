using PredictLottoNZ.Models.DTOs;

namespace PredictLottoNZ.Services;

public class FrequencyPredictionProvider : IPredictionProvider
{
    private readonly IFrequencyCalculationService _frequencyService;
    private readonly ILogger<FrequencyPredictionProvider> _logger;
    
    public string ProviderName => "Frequency";
    public int Priority => 100; // Lowest priority - fallback provider
    
    public FrequencyPredictionProvider(
        IFrequencyCalculationService frequencyService,
        ILogger<FrequencyPredictionProvider> logger)
    {
        _frequencyService = frequencyService;
        _logger = logger;
    }
    
    public async Task<IEnumerable<PredictionResult>> PredictAsync(int count)
    {
        _logger.LogInformation("Generating {Count} frequency-based predictions", count);
        
        try
        {
            var predictions = await _frequencyService.GenerateTopPredictionsAsync(count);
            
            _logger.LogInformation("Successfully generated {Count} frequency-based predictions", predictions.Count());
            
            return predictions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate frequency-based predictions");
            throw;
        }
    }
}