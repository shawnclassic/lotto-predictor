using Microsoft.EntityFrameworkCore;
using PredictLottoNZ.Data;
using PredictLottoNZ.Models.DTOs;

namespace PredictLottoNZ.Services;

public interface ILottoQueryService
{
    Task<bool> DrawExistsAsync(int drawNumber);
    Task<LottoDrawDto?> GetLatestDrawAsync();
}

public class LottoQueryService : ILottoQueryService
{
    private readonly LottoDbContext _context;
    private readonly ILogger<LottoQueryService> _logger;

    public LottoQueryService(LottoDbContext context, ILogger<LottoQueryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> DrawExistsAsync(int drawNumber)
    {
        try
        {
            _logger.LogDebug("Checking if draw {DrawNumber} exists", drawNumber);
            
            if (drawNumber <= 0)
            {
                _logger.LogWarning("Invalid draw number {DrawNumber} provided", drawNumber);
                return false;
            }

            var exists = await _context.LottoDraws
                .AnyAsync(d => d.Draw == drawNumber);

            _logger.LogDebug("Draw {DrawNumber} exists: {Exists}", drawNumber, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if draw {DrawNumber} exists", drawNumber);
            throw;
        }
    }

    public async Task<LottoDrawDto?> GetLatestDrawAsync()
    {
        try
        {
            _logger.LogDebug("Retrieving latest lottery draw");

            var latestDraw = await _context.LottoDraws
                .OrderByDescending(d => d.Date)
                .ThenByDescending(d => d.Draw)
                .FirstOrDefaultAsync();

            if (latestDraw == null)
            {
                _logger.LogInformation("No lottery draws found in database");
                return null;
            }

            _logger.LogDebug("Latest draw found: {DrawNumber} from {Date}", 
                latestDraw.Draw, latestDraw.Date);

            return LottoDrawDto.FromEntity(latestDraw);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest lottery draw");
            throw;
        }
    }
}