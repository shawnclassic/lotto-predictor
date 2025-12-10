namespace PredictLottoNZ.Models.DTOs;

public class ImportResult
{
    public int RecordsAdded { get; set; }
    public int RecordsSkipped { get; set; }
    public List<string> Errors { get; set; } = new();
    
    public int TotalRecords => RecordsAdded + RecordsSkipped;
    public bool HasErrors => Errors.Any();
}