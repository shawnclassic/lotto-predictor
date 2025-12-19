using Microsoft.AspNetCore.Mvc;
using PredictLottoNZ.Models.DTOs;
using PredictLottoNZ.Services;

namespace PredictLottoNZ.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PredictionsController : ControllerBase
{
    private readonly IPredictionService _predictionService;
    private readonly ILogger<PredictionsController> _logger;
    
    public PredictionsController(
        IPredictionService predictionService,
        ILogger<PredictionsController> logger)
    {
        _predictionService = predictionService;
        _logger = logger;
    }
    
    /// <summary>
    /// Generate new predictions using the prediction provider chain
    /// </summary>
    /// <param name="count">Number of predictions to generate (1-10)</param>
    /// <returns>Generated predictions</returns>
    [HttpGet("generate")]
    public async Task<ActionResult<IEnumerable<PredictionResult>>> GeneratePredictions([FromQuery] int count = 5)
    {
        try
        {
            if (count <= 0 || count > 10)
            {
                return BadRequest("Count must be between 1 and 10");
            }
            
            _logger.LogInformation("Generating {Count} predictions", count);
            
            var predictions = await _predictionService.GeneratePredictionsAsync(count);
            
            return Ok(predictions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate predictions");
            return StatusCode(500, "Failed to generate predictions");
        }
    }
    
    /// <summary>
    /// Get stored predictions from the database
    /// </summary>
    /// <param name="count">Number of predictions to retrieve (1-50)</param>
    /// <returns>Stored predictions</returns>
    [HttpGet("stored")]
    public async Task<ActionResult<IEnumerable<PredictionResult>>> GetStoredPredictions([FromQuery] int count = 10)
    {
        try
        {
            if (count <= 0 || count > 50)
            {
                return BadRequest("Count must be between 1 and 50");
            }
            
            _logger.LogInformation("Retrieving {Count} stored predictions", count);
            
            var predictions = await _predictionService.GetStoredPredictionsAsync(count);
            
            return Ok(predictions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve stored predictions");
            return StatusCode(500, "Failed to retrieve stored predictions");
        }
    }
    
    /// <summary>
    /// Get paginated stored predictions from the database
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated stored predictions</returns>
    [HttpGet("stored/paginated")]
    public async Task<ActionResult<PaginatedResponse<PredictionResult>>> GetStoredPredictionsPaginated([FromQuery] PredictionPaginationRequest request)
    {
        try
        {
            _logger.LogInformation("Retrieving paginated stored predictions - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);
            
            var paginatedPredictions = await _predictionService.GetStoredPredictionsPaginatedAsync(request);
            
            return Ok(paginatedPredictions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve paginated stored predictions");
            return StatusCode(500, "Failed to retrieve paginated stored predictions");
        }
    }
}