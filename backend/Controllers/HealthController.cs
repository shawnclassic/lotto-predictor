using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PredictLottoNZ.Data;

namespace PredictLottoNZ.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly LottoDbContext _context;
    private readonly ILogger<HealthController> _logger;
    
    public HealthController(LottoDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Check database connectivity
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return StatusCode(503, new { status = "Unhealthy", message = "Cannot connect to database" });
            }
            
            // Get basic statistics
            var stats = new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                database = new
                {
                    connected = true,
                    lottoDraws = await _context.LottoDraws.CountAsync(),
                    numberCombinations = await _context.NumberCombinations.CountAsync(),
                    predictions = await _context.Predictions.CountAsync()
                }
            };
            
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new { status = "Unhealthy", message = ex.Message });
        }
    }
}