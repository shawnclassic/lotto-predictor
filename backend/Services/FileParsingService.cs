using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using PredictLottoNZ.Models;

namespace PredictLottoNZ.Services;

public interface IFileParsingService
{
    Task<IEnumerable<int[]>> ParseFileAsync(Stream fileStream, string fileName);
    bool IsSupportedFileType(string fileName);
}

public class FileParsingService : IFileParsingService
{
    private readonly ILogger<FileParsingService> _logger;
    
    // Regex pattern to match 6 numbers separated by commas, spaces, or other delimiters
    private static readonly Regex NumberPattern = new(@"\b(\d{1,2})\s*[,\s]\s*(\d{1,2})\s*[,\s]\s*(\d{1,2})\s*[,\s]\s*(\d{1,2})\s*[,\s]\s*(\d{1,2})\s*[,\s]\s*(\d{1,2})\b", 
        RegexOptions.Compiled | RegexOptions.Multiline);
    
    // Alternative pattern for line-based numbers (one number per line, 6 consecutive lines)
    private static readonly Regex SingleNumberPattern = new(@"^\s*(\d{1,2})\s*$", 
        RegexOptions.Compiled | RegexOptions.Multiline);

    public FileParsingService(ILogger<FileParsingService> logger)
    {
        _logger = logger;
    }

    public bool IsSupportedFileType(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;
            
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".csv" or ".txt" or ".pdf";
    }

    public async Task<IEnumerable<int[]>> ParseFileAsync(Stream fileStream, string fileName)
    {
        if (!IsSupportedFileType(fileName))
        {
            throw new NotSupportedException($"File type not supported: {Path.GetExtension(fileName)}");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".csv" => await ParseCsvAsync(fileStream),
            ".txt" => await ParseTxtAsync(fileStream),
            ".pdf" => await ParsePdfAsync(fileStream),
            _ => throw new NotSupportedException($"File type not supported: {extension}")
        };
    }

    private async Task<IEnumerable<int[]>> ParseCsvAsync(Stream csvStream)
    {
        var combinations = new List<int[]>();
        
        using var reader = new StreamReader(csvStream);
        string? line;
        var lineNumber = 0;
        
        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNumber++;
            
            if (string.IsNullOrWhiteSpace(line))
                continue;
                
            try
            {
                // Try to parse as comma-separated values
                var numbers = ParseCommaSeparatedNumbers(line);
                if (numbers != null)
                {
                    combinations.Add(numbers);
                    continue;
                }
                
                // Try to extract numbers using regex
                var regexNumbers = ExtractNumbersFromLine(line);
                if (regexNumbers != null)
                {
                    combinations.Add(regexNumbers);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse CSV line {LineNumber}: {Line}", lineNumber, line);
            }
        }
        
        _logger.LogInformation("Parsed {Count} combinations from CSV file", combinations.Count);
        return combinations;
    }

    private async Task<IEnumerable<int[]>> ParseTxtAsync(Stream txtStream)
    {
        var combinations = new List<int[]>();
        
        using var reader = new StreamReader(txtStream);
        var content = await reader.ReadToEndAsync();
        
        // First, try to find complete 6-number combinations in single lines
        var lineMatches = NumberPattern.Matches(content);
        foreach (Match match in lineMatches)
        {
            try
            {
                var numbers = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    numbers[i] = int.Parse(match.Groups[i + 1].Value);
                }
                
                if (IsValidCombination(numbers))
                {
                    combinations.Add(numbers);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse number combination from match: {Match}", match.Value);
            }
        }
        
        // If no complete combinations found, try line-by-line approach
        if (combinations.Count == 0)
        {
            combinations.AddRange(await ParseLineByLineNumbers(content));
        }
        
        _logger.LogInformation("Parsed {Count} combinations from TXT file", combinations.Count);
        return combinations;
    }

    private async Task<IEnumerable<int[]>> ParsePdfAsync(Stream pdfStream)
    {
        var combinations = new List<int[]>();
        
        try
        {
            using var pdfReader = new PdfReader(pdfStream);
            using var pdfDocument = new PdfDocument(pdfReader);
            
            var textContent = "";
            
            // Extract text from all pages
            for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
            {
                var strategy = new SimpleTextExtractionStrategy();
                var pageText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), strategy);
                textContent += pageText + "\n";
            }
            
            // Parse the extracted text using the same logic as TXT files
            var lineMatches = NumberPattern.Matches(textContent);
            foreach (Match match in lineMatches)
            {
                try
                {
                    var numbers = new int[6];
                    for (int i = 0; i < 6; i++)
                    {
                        numbers[i] = int.Parse(match.Groups[i + 1].Value);
                    }
                    
                    if (IsValidCombination(numbers))
                    {
                        combinations.Add(numbers);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse number combination from PDF match: {Match}", match.Value);
                }
            }
            
            // If no complete combinations found, try line-by-line approach
            if (combinations.Count == 0)
            {
                combinations.AddRange(await ParseLineByLineNumbers(textContent));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse PDF file");
            throw new InvalidOperationException("Failed to parse PDF file", ex);
        }
        
        _logger.LogInformation("Parsed {Count} combinations from PDF file", combinations.Count);
        return combinations;
    }

    private int[]? ParseCommaSeparatedNumbers(string line)
    {
        var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length != 6)
            return null;
            
        var numbers = new int[6];
        
        for (int i = 0; i < 6; i++)
        {
            if (!int.TryParse(parts[i].Trim(), out numbers[i]))
                return null;
        }
        
        return IsValidCombination(numbers) ? numbers : null;
    }

    private int[]? ExtractNumbersFromLine(string line)
    {
        var match = NumberPattern.Match(line);
        if (!match.Success)
            return null;
            
        var numbers = new int[6];
        for (int i = 0; i < 6; i++)
        {
            if (!int.TryParse(match.Groups[i + 1].Value, out numbers[i]))
                return null;
        }
        
        return IsValidCombination(numbers) ? numbers : null;
    }

    private async Task<IEnumerable<int[]>> ParseLineByLineNumbers(string content)
    {
        var combinations = new List<int[]>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var currentNumbers = new List<int>();
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine))
                continue;
                
            var match = SingleNumberPattern.Match(trimmedLine);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var number))
            {
                if (number >= 1 && number <= 40)
                {
                    currentNumbers.Add(number);
                    
                    // If we have 6 numbers, create a combination
                    if (currentNumbers.Count == 6)
                    {
                        var combination = currentNumbers.ToArray();
                        if (IsValidCombination(combination))
                        {
                            combinations.Add(combination);
                        }
                        currentNumbers.Clear();
                    }
                }
                else
                {
                    // Invalid number, reset current collection
                    currentNumbers.Clear();
                }
            }
            else
            {
                // Line doesn't contain a single number, reset current collection
                currentNumbers.Clear();
            }
        }
        
        return combinations;
    }

    private bool IsValidCombination(int[] numbers)
    {
        if (numbers.Length != 6)
            return false;
            
        // Check all numbers are in valid range [1, 40]
        if (numbers.Any(n => n < 1 || n > 40))
            return false;
            
        // Check all numbers are unique
        return numbers.Distinct().Count() == 6;
    }
}