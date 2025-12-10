using Microsoft.EntityFrameworkCore;
using PredictLottoNZ.Data;
using PredictLottoNZ.Models;
using PredictLottoNZ.Models.DTOs;

namespace PredictLottoNZ.Services;

public interface ICombinationService
{
    Task<ImportResult> ImportCombinationsAsync(Stream fileStream, string fileName);
    Task<ImportResult> ImportCombinationsAsync(IEnumerable<int[]> combinations);
    Task<IEnumerable<NumberCombination>> GetCombinationsAsync(int skip = 0, int take = 100);
    Task<int> GetCombinationCountAsync();
    bool ValidateCombination(int[] numbers);
    bool ValidateFileType(string fileName);
}

public class CombinationService : ICombinationService
{
    private readonly LottoDbContext _context;
    private readonly IFileParsingService _fileParsingService;
    private readonly ILogger<CombinationService> _logger;

    public CombinationService(
        LottoDbContext context,
        IFileParsingService fileParsingService,
        ILogger<CombinationService> logger)
    {
        _context = context;
        _fileParsingService = fileParsingService;
        _logger = logger;
    }

    public async Task<ImportResult> ImportCombinationsAsync(Stream fileStream, string fileName)
    {
        if (!_fileParsingService.IsSupportedFileType(fileName))
        {
            throw new NotSupportedException($"File type not supported: {Path.GetExtension(fileName)}");
        }

        try
        {
            var combinations = await _fileParsingService.ParseFileAsync(fileStream, fileName);
            return await ImportCombinationsAsync(combinations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import combinations from file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<ImportResult> ImportCombinationsAsync(IEnumerable<int[]> combinations)
    {
        var result = new ImportResult();
        var validCombinations = new List<NumberCombination>();
        var existingCombinations = new HashSet<string>();

        // Load existing combinations to check for duplicates
        var existing = await _context.NumberCombinations
            .Select(nc => $"{nc.Number1},{nc.Number2},{nc.Number3},{nc.Number4},{nc.Number5},{nc.Number6}")
            .ToListAsync();
        
        foreach (var combo in existing)
        {
            existingCombinations.Add(combo);
        }

        foreach (var combination in combinations)
        {
            try
            {
                // Validate the combination
                if (!ValidateCombination(combination))
                {
                    result.RecordsSkipped++;
                    _logger.LogWarning("Invalid combination skipped: [{Numbers}]", string.Join(", ", combination));
                    continue;
                }

                // Sort numbers for consistent duplicate detection
                var sortedNumbers = combination.OrderBy(n => n).ToArray();
                var combinationKey = string.Join(",", sortedNumbers);

                // Check for duplicates
                if (existingCombinations.Contains(combinationKey))
                {
                    result.RecordsSkipped++;
                    _logger.LogDebug("Duplicate combination skipped: [{Numbers}]", string.Join(", ", sortedNumbers));
                    continue;
                }

                // Create new combination entity
                var numberCombination = new NumberCombination();
                numberCombination.SetNumbers(sortedNumbers);
                numberCombination.CreatedAt = DateTime.UtcNow;

                validCombinations.Add(numberCombination);
                existingCombinations.Add(combinationKey);
                result.RecordsAdded++;
            }
            catch (Exception ex)
            {
                result.RecordsSkipped++;
                _logger.LogWarning(ex, "Failed to process combination: [{Numbers}]", string.Join(", ", combination));
            }
        }

        // Batch insert valid combinations
        if (validCombinations.Count > 0)
        {
            try
            {
                await BatchInsertCombinationsAsync(validCombinations);
                _logger.LogInformation("Successfully imported {Count} number combinations", validCombinations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save combinations to database");
                throw;
            }
        }

        return result;
    }

    public async Task<IEnumerable<NumberCombination>> GetCombinationsAsync(int skip = 0, int take = 100)
    {
        return await _context.NumberCombinations
            .OrderByDescending(nc => nc.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetCombinationCountAsync()
    {
        return await _context.NumberCombinations.CountAsync();
    }

    public bool ValidateCombination(int[] numbers)
    {
        // Must have exactly 6 numbers
        if (numbers == null || numbers.Length != 6)
        {
            return false;
        }

        // All numbers must be in range [1, 40]
        if (numbers.Any(n => n < 1 || n > 40))
        {
            return false;
        }

        // All numbers must be unique
        if (numbers.Distinct().Count() != 6)
        {
            return false;
        }

        return true;
    }

    public bool ValidateFileType(string fileName)
    {
        return _fileParsingService.IsSupportedFileType(fileName);
    }

    private async Task BatchInsertCombinationsAsync(List<NumberCombination> combinations)
    {
        const int batchSize = 1000;
        
        for (int i = 0; i < combinations.Count; i += batchSize)
        {
            var batch = combinations.Skip(i).Take(batchSize).ToList();
            
            try
            {
                _context.NumberCombinations.AddRange(batch);
                await _context.SaveChangesAsync();
                
                _logger.LogDebug("Saved batch of {Count} combinations (batch {BatchNumber})", 
                    batch.Count, (i / batchSize) + 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save batch {BatchNumber} of combinations", (i / batchSize) + 1);
                
                // Try to save individual records to identify problematic ones
                await SaveIndividualCombinations(batch);
            }
        }
    }

    private async Task SaveIndividualCombinations(List<NumberCombination> combinations)
    {
        foreach (var combination in combinations)
        {
            try
            {
                _context.NumberCombinations.Add(combination);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save individual combination: [{Numbers}]", 
                    string.Join(", ", combination.GetNumbers()));
                
                // Remove the failed entity from the context
                _context.Entry(combination).State = EntityState.Detached;
            }
        }
    }
}