using PredictLottoNZ.Models;

namespace PredictLottoNZ.Models.DTOs;

public class PredictionResult
{
    public int Id { get; set; }
    public int[] Numbers { get; set; } = Array.Empty<int>();
    public double Score { get; set; }
    public double? UpdatedScore { get; set; }
    public DateTime? LastScoreUpdate { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public double? ConfidenceScore { get; set; }
    public string ReasoningExplanation { get; set; } = string.Empty;
    public bool HasScoreUpdates { get; set; }
    
    public static PredictionResult FromEntity(Prediction prediction)
    {
        return new PredictionResult
        {
            Id = prediction.Id,
            Numbers = prediction.GetNumbers(),
            Score = prediction.Score ?? 0,
            UpdatedScore = prediction.UpdatedScore,
            LastScoreUpdate = prediction.LastScoreUpdate,
            Source = prediction.Source,
            CreatedAt = prediction.CreatedAt,
            ConfidenceScore = prediction.ConfidenceScore,
            ReasoningExplanation = prediction.ReasoningExplanation,
            HasScoreUpdates = prediction.UpdatedScore.HasValue
        };
    }
}