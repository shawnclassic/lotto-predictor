using PredictLottoNZ.Models.DTOs;

namespace PredictLottoNZ.Services;

public interface IPredictionProvider
{
    Task<IEnumerable<PredictionResult>> PredictAsync(int count);
    string ProviderName { get; }
    int Priority { get; } // Lower numbers = higher priority
}