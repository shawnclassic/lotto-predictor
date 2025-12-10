using Microsoft.AspNetCore.Mvc;
using PredictLottoNZ.Models.DTOs;
using PredictLottoNZ.Services;

namespace PredictLottoNZ.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CombinationsController : ControllerBase
{
    private readonly ICombinationService _combinationService;
    private readonly IPredictionService _predictionService;
    private readonly ILogger<CombinationsController> _logger;

    public CombinationsController(
        ICombinationService combinationService,
        IPredictionService predictionService,
        ILogger<CombinationsController> logger)
    {
        _combinationService = combinationService;
        _predictionService = predictionService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a file containing number combinations (CSV, TXT, or PDF)
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <returns>Import result with counts of added and skipped records</returns>
    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public async Task<ActionResult<ImportResult>> UploadCombinations(IFormFile file)
    {
        try
        {
            _logger.LogInformation("Received combination file upload request for file: {FileName}", file?.FileName);

            // Validate file presence
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file provided in upload request");
                return BadRequest(new { error = "No file provided or file is empty" });
            }

            // Validate file size (max 10MB)
            const long maxFileSize = 10 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                _logger.LogWarning("File too large: {FileSize} bytes, max allowed: {MaxSize} bytes", 
                    file.Length, maxFileSize);
                return BadRequest(new { error = $"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB" });
            }

            // Validate file type
            if (!_combinationService.ValidateFileType(file.FileName))
            {
                _logger.LogWarning("Invalid file type uploaded: {FileName}", file.FileName);
                return BadRequest(new { error = "Unsupported file type. Only CSV, TXT, and PDF files are supported." });
            }

            _logger.LogInformation("Processing combination file: {FileName}, Size: {FileSize} bytes", 
                file.FileName, file.Length);

            // Process the file
            using var stream = file.OpenReadStream();
            var result = await _combinationService.ImportCombinationsAsync(stream, file.FileName);
            
            _logger.LogInformation("Combination file upload completed: {FileName}, Added: {Added}, Skipped: {Skipped}, Errors: {ErrorCount}", 
                file.FileName, result.RecordsAdded, result.RecordsSkipped, result.Errors.Count);
            
            // Return appropriate status based on results
            if (result.HasErrors && result.RecordsAdded == 0)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning(ex, "Unsupported file type: {FileName}", file?.FileName);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing combination file upload: {FileName}", file?.FileName);
            return StatusCode(500, new { error = "Internal server error occurred during file processing" });
        }
    }

    /// <summary>
    /// Get paginated list of number combinations
    /// </summary>
    /// <param name="skip">Number of records to skip</param>
    /// <param name="take">Number of records to take (max 100)</param>
    /// <returns>List of number combinations</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetCombinations(int skip = 0, int take = 50)
    {
        if (take > 100)
        {
            take = 100;
        }

        try
        {
            var combinations = await _combinationService.GetCombinationsAsync(skip, take);
            var result = combinations.Select(c => new
            {
                c.Id,
                Numbers = c.GetNumbers(),
                c.CreatedAt
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve combinations");
            return StatusCode(500, "An error occurred while retrieving combinations");
        }
    }

    /// <summary>
    /// Get total count of number combinations
    /// </summary>
    /// <returns>Total count</returns>
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCombinationCount()
    {
        try
        {
            var count = await _combinationService.GetCombinationCountAsync();
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get combination count");
            return StatusCode(500, "An error occurred while getting combination count");
        }
    }

    /// <summary>
    /// Validate a number combination
    /// </summary>
    /// <param name="numbers">Array of 6 numbers to validate</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    public ActionResult<object> ValidateCombination([FromBody] int[] numbers)
    {
        try
        {
            var isValid = _combinationService.ValidateCombination(numbers);
            var result = new
            {
                IsValid = isValid,
                Numbers = numbers,
                Message = isValid ? "Valid combination" : GetValidationMessage(numbers)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate combination");
            return StatusCode(500, "An error occurred while validating the combination");
        }
    }

    /// <summary>
    /// Generate predictions using the prediction provider chain
    /// </summary>
    /// <param name="count">Number of predictions to generate (1-10)</param>
    /// <returns>Generated predictions</returns>
    [HttpGet("predictions")]
    public async Task<ActionResult<IEnumerable<PredictionResult>>> GetPredictions([FromQuery] int count = 5)
    {
        try
        {
            _logger.LogDebug("Generating {Count} predictions", count);

            // Validate count parameter
            if (count <= 0 || count > 10)
            {
                _logger.LogWarning("Invalid prediction count requested: {Count}", count);
                return BadRequest(new { error = "Count must be between 1 and 10" });
            }
            
            _logger.LogInformation("Generating {Count} predictions using prediction service", count);
            
            var predictions = await _predictionService.GeneratePredictionsAsync(count);
            
            _logger.LogInformation("Successfully generated {Count} predictions", predictions.Count());
            
            return Ok(predictions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating predictions");
            return StatusCode(500, new { error = "Internal server error occurred while generating predictions" });
        }
    }

    private string GetValidationMessage(int[] numbers)
    {
        if (numbers == null || numbers.Length != 6)
        {
            return "Must provide exactly 6 numbers";
        }

        if (numbers.Any(n => n < 1 || n > 40))
        {
            return "All numbers must be between 1 and 40";
        }

        if (numbers.Distinct().Count() != 6)
        {
            return "All numbers must be unique";
        }

        return "Invalid combination";
    }
}