using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PredictLottoNZ.Models;

[Table("ExternalServiceCallLogs")]
public class ExternalServiceCallLog
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    [MaxLength(100)]
    public string ServiceName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Endpoint { get; set; } = string.Empty;
    
    [Column(TypeName = "text")]
    public string RequestPayload { get; set; } = string.Empty;
    
    [Column(TypeName = "text")]
    public string ResponsePayload { get; set; } = string.Empty;
    
    [Required]
    public bool Success { get; set; }
    
    [Required]
    public TimeSpan Duration { get; set; }
    
    [Column(TypeName = "text")]
    public string? ErrorMessage { get; set; }
}