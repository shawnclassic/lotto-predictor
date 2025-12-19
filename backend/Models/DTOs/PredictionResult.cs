using PredictLottoNZ.Models;

namespace PredictLottoNZ.Models.DTOs;

public class PredictionResult
{
    public int[] Numbers { get; set; } = Array.Empty<int>();
    public double Score { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public double? ConfidenceScore { get; set; }
    public string ReasoningExplanation { get; set; } = string.Empty;
    
    public static PredictionResult FromEntity(Prediction prediction)
    {
        return new PredictionResult
        {
            Numbers = prediction.GetNumbers(),
            Score = prediction.Score ?? 0,
            Source = prediction.Source,
            CreatedAt = prediction.CreatedAt,
            ConfidenceScore = prediction.ConfidenceScore,
            ReasoningExplanation = prediction.ReasoningExplanation
        };
    }
}