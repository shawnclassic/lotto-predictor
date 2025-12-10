namespace PredictLottoNZ.Models.DTOs;

public class TrainingDataExport
{
    public DateTime ExportedAt { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<TrainingDataPrediction> Predictions { get; set; } = new();
    public List<TrainingDataServiceCall> ExternalServiceCalls { get; set; } = new();
    public List<TrainingDataLottoDraw> LottoDraws { get; set; } = new();
    public List<TrainingDataCombination> NumberCombinations { get; set; } = new();
}

public class TrainingDataPrediction
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Source { get; set; } = string.Empty;
    public int[] Numbers { get; set; } = Array.Empty<int>();
    public double? Score { get; set; }
    public string? RawRequestPayload { get; set; }
    public string? RawResponsePayload { get; set; }
}

public class TrainingDataServiceCall
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public bool Success { get; set; }
    public TimeSpan Duration { get; set; }
    public string RequestPayload { get; set; } = string.Empty;
    public string ResponsePayload { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class TrainingDataLottoDraw
{
    public int Draw { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public int[] WinningNumbers { get; set; } = Array.Empty<int>();
    public int BonusNumber { get; set; }
    public int Powerball { get; set; }
    public Dictionary<int, decimal?> PrizeDivisions { get; set; } = new();
}

public class TrainingDataCombination
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int[] Numbers { get; set; } = Array.Empty<int>();
}

public class PredictionAccuracyReport
{
    public DateTime AnalyzedAt { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int TotalPredictions { get; set; }
    public int TotalComparisons { get; set; }
    public List<PredictionAccuracyMetric> AccuracyMetrics { get; set; } = new();
    public Dictionary<string, PredictionSourceSummary> SummaryBySource { get; set; } = new();
}

public class PredictionAccuracyMetric
{
    public int PredictionId { get; set; }
    public string PredictionSource { get; set; } = string.Empty;
    public DateTime PredictionCreatedAt { get; set; }
    public int DrawNumber { get; set; }
    public DateTime DrawDate { get; set; }
    public int MatchingNumbers { get; set; }
    public bool BonusMatch { get; set; }
    public bool PowerballMatch { get; set; }
    public double? PredictionScore { get; set; }
    public int DaysUntilDraw { get; set; }
}

public class PredictionSourceSummary
{
    public string Source { get; set; } = string.Empty;
    public int TotalPredictions { get; set; }
    public int TotalComparisons { get; set; }
    public double AverageMatchingNumbers { get; set; }
    public int BestMatch { get; set; }
    public int BonusMatches { get; set; }
    public int PowerballMatches { get; set; }
    public double AverageScore { get; set; }
}