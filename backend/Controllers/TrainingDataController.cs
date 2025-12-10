using Microsoft.AspNetCore.Mvc;
using PredictLottoNZ.Models.DTOs;
using PredictLottoNZ.Services;
using System.Text.Json;

namespace PredictLottoNZ.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrainingDataController : ControllerBase
{
    private readonly ITrainingDataService _trainingDataService;
    private readonly ILogger<TrainingDataController> _logger;
    
    public TrainingDataController(ITrainingDataService trainingDataService, ILogger<TrainingDataController> logger)
    {
        _trainingDataService = trainingDataService;
        _logger = logger;
    }
    
    /// <summary>
    /// Export comprehensive training data for ML model development
    /// </summary>
    /// <param name="fromDate">Optional start date for data export</param>
    /// <param name="toDate">Optional end date for data export</param>
    /// <returns>Complete training data export</returns>
    [HttpGet("export")]
    public async Task<ActionResult<TrainingDataExport>> ExportTrainingData(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            _logger.LogInformation("Exporting training data from {FromDate} to {ToDate}", fromDate, toDate);
            
            var export = await _trainingDataService.ExportTrainingDataAsync(fromDate, toDate);
            
            _logger.LogInformation("Successfully exported training data with {PredictionCount} predictions, {ServiceCallCount} service calls",
                export.Predictions.Count, export.ExternalServiceCalls.Count);
            
            return Ok(export);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export training data");
            return StatusCode(500, new { error = "Failed to export training data", details = ex.Message });
        }
    }
    
    /// <summary>
    /// Export training data as downloadable JSON file
    /// </summary>
    /// <param name="fromDate">Optional start date for data export</param>
    /// <param name="toDate">Optional end date for data export</param>
    /// <returns>JSON file download</returns>
    [HttpGet("export/download")]
    public async Task<IActionResult> DownloadTrainingData(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            _logger.LogInformation("Downloading training data from {FromDate} to {ToDate}", fromDate, toDate);
            
            var export = await _trainingDataService.ExportTrainingDataAsync(fromDate, toDate);
            
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(export, jsonOptions);
            
            var fileName = $"training-data-{DateTime.UtcNow:yyyy-MM-dd-HHmmss}.json";
            
            _logger.LogInformation("Generated training data file {FileName} with {Size} bytes", fileName, jsonBytes.Length);
            
            return File(jsonBytes, "application/json", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download training data");
            return StatusCode(500, new { error = "Failed to download training data", details = ex.Message });
        }
    }
    
    /// <summary>
    /// Analyze prediction accuracy against historical lottery draws
    /// </summary>
    /// <param name="fromDate">Optional start date for analysis</param>
    /// <param name="toDate">Optional end date for analysis</param>
    /// <returns>Prediction accuracy analysis report</returns>
    [HttpGet("accuracy")]
    public async Task<ActionResult<PredictionAccuracyReport>> AnalyzePredictionAccuracy(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            _logger.LogInformation("Analyzing prediction accuracy from {FromDate} to {ToDate}", fromDate, toDate);
            
            var report = await _trainingDataService.AnalyzePredictionAccuracyAsync(fromDate, toDate);
            
            _logger.LogInformation("Successfully analyzed prediction accuracy for {TotalPredictions} predictions across {SourceCount} sources",
                report.TotalPredictions, report.SummaryBySource.Count);
            
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze prediction accuracy");
            return StatusCode(500, new { error = "Failed to analyze prediction accuracy", details = ex.Message });
        }
    }
    
    /// <summary>
    /// Get external service call logs for debugging and analysis
    /// </summary>
    /// <param name="serviceName">Optional filter by service name</param>
    /// <param name="fromDate">Optional start date for logs</param>
    /// <param name="toDate">Optional end date for logs</param>
    /// <param name="limit">Maximum number of logs to return (default: 100)</param>
    /// <returns>External service call logs</returns>
    [HttpGet("service-calls")]
    public async Task<ActionResult<IEnumerable<object>>> GetServiceCallLogs(
        [FromQuery] string? serviceName = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int limit = 100)
    {
        try
        {
            _logger.LogInformation("Retrieving service call logs for service {ServiceName} from {FromDate} to {ToDate}",
                serviceName, fromDate, toDate);
            
            var logs = await _trainingDataService.GetServiceCallLogsAsync(serviceName, fromDate, toDate);
            
            // Limit results and exclude potentially large payloads for summary view
            var summaryLogs = logs
                .Take(limit)
                .Select(log => new
                {
                    log.Id,
                    log.CreatedAt,
                    log.ServiceName,
                    log.Endpoint,
                    log.Success,
                    Duration = log.Duration.TotalMilliseconds,
                    log.ErrorMessage,
                    HasRequestPayload = !string.IsNullOrEmpty(log.RequestPayload),
                    HasResponsePayload = !string.IsNullOrEmpty(log.ResponsePayload)
                });
            
            _logger.LogInformation("Retrieved {LogCount} service call logs", summaryLogs.Count());
            
            return Ok(summaryLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve service call logs");
            return StatusCode(500, new { error = "Failed to retrieve service call logs", details = ex.Message });
        }
    }
    
    /// <summary>
    /// Get detailed service call log including full payloads
    /// </summary>
    /// <param name="id">Service call log ID</param>
    /// <returns>Detailed service call log</returns>
    [HttpGet("service-calls/{id}")]
    public async Task<ActionResult<object>> GetServiceCallLogDetail(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving detailed service call log {Id}", id);
            
            var logs = await _trainingDataService.GetServiceCallLogsAsync();
            var log = logs.FirstOrDefault(l => l.Id == id);
            
            if (log == null)
            {
                _logger.LogWarning("Service call log {Id} not found", id);
                return NotFound(new { error = "Service call log not found" });
            }
            
            var detailedLog = new
            {
                log.Id,
                log.CreatedAt,
                log.ServiceName,
                log.Endpoint,
                log.Success,
                Duration = log.Duration.TotalMilliseconds,
                log.ErrorMessage,
                log.RequestPayload,
                log.ResponsePayload
            };
            
            _logger.LogInformation("Retrieved detailed service call log {Id}", id);
            
            return Ok(detailedLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve detailed service call log {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve service call log", details = ex.Message });
        }
    }
}