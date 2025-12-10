using System.Globalization;
using System.Reflection;
using PredictLottoNZ.Models;

namespace PredictLottoNZ.Services;

public interface ICsvParsingService
{
    Task<IEnumerable<LottoDraw>> ParseCsvAsync(Stream csvStream);
}

public class CsvParsingService : ICsvParsingService
{
    private readonly ILogger<CsvParsingService> _logger;
    
    // Header mapping dictionary for transforming CSV headers to property names
    private static readonly Dictionary<string, string> HeaderMappings = new()
    {
        // Core fields
        { "Draw", "Draw" },
        { "Date", "Date" },
        { "Winning Number 1", "WinningNumber1" },
        { "Winning Number 2", "WinningNumber2" },
        { "Winning Number 3", "WinningNumber3" },
        { "Winning Number 4", "WinningNumber4" },
        { "Winning Number 5", "WinningNumber5" },
        { "Winning Number 6", "WinningNumber6" },
        { "Bonus Number", "BonusNumber" },
        { "Powerball", "Powerball" },
        { "From Last", "FromLast" },
        
        // Statistical interval mappings
        { "1-Oct", "OneToTen" },
        { "Nov-20", "ElevenToTwenty" },
        { "21-30", "TwentyOneToThirty" },
        { "31-40", "ThirtyOneToForty" },
        
        // Prize division mappings
        { "Division 1 Prize", "Division1Prize" },
        { "Division 1 Winners", "Division1Winners" },
        { "Division 2 Prize", "Division2Prize" },
        { "Division 2 Winners", "Division2Winners" },
        { "Division 3 Prize", "Division3Prize" },
        { "Division 3 Winners", "Division3Winners" },
        { "Division 4 Prize", "Division4Prize" },
        { "Division 4 Winners", "Division4Winners" },
        { "Division 5 Prize", "Division5Prize" },
        { "Division 5 Winners", "Division5Winners" },
        { "Division 6 Prize", "Division6Prize" },
        { "Division 6 Winners", "Division6Winners" }
    };

    public CsvParsingService(ILogger<CsvParsingService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<LottoDraw>> ParseCsvAsync(Stream csvStream)
    {
        var results = new List<LottoDraw>();
        
        using var reader = new StreamReader(csvStream);
        
        // Read header line
        var headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrEmpty(headerLine))
        {
            _logger.LogWarning("CSV file is empty or has no header");
            return results;
        }

        var headers = ParseCsvLine(headerLine);
        var propertyMappings = CreatePropertyMappings(headers);
        
        var lineNumber = 1;
        string? line;
        
        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNumber++;
            
            if (string.IsNullOrWhiteSpace(line))
                continue;
                
            try
            {
                var values = ParseCsvLine(line);
                var lottoDraw = ParseLottoDrawFromValues(values, propertyMappings, lineNumber);
                
                if (lottoDraw != null)
                {
                    results.Add(lottoDraw);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse line {LineNumber}: {Line}", lineNumber, line);
                // Continue processing other lines
            }
        }
        
        _logger.LogInformation("Successfully parsed {Count} lottery draws from CSV", results.Count);
        return results;
    }

    private string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var inQuotes = false;
        var currentValue = "";
        
        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.Trim());
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }
        
        // Add the last value
        values.Add(currentValue.Trim());
        
        return values.ToArray();
    }

    private Dictionary<string, int> CreatePropertyMappings(string[] headers)
    {
        var mappings = new Dictionary<string, int>();
        
        for (int i = 0; i < headers.Length; i++)
        {
            var header = headers[i].Trim();
            
            // Try direct mapping first
            if (HeaderMappings.ContainsKey(header))
            {
                var propertyName = HeaderMappings[header];
                mappings[propertyName] = i;
                continue;
            }
            
            // Try space removal and PascalCase conversion
            var transformedHeader = TransformHeaderToPascalCase(header);
            if (HasProperty<LottoDraw>(transformedHeader))
            {
                mappings[transformedHeader] = i;
                continue;
            }
            
            _logger.LogDebug("Unmapped CSV header: {Header}", header);
        }
        
        return mappings;
    }

    private string TransformHeaderToPascalCase(string header)
    {
        if (string.IsNullOrEmpty(header))
            return header;
            
        // Remove spaces and convert to PascalCase
        var words = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = "";
        
        foreach (var word in words)
        {
            if (!string.IsNullOrEmpty(word))
            {
                result += char.ToUpper(word[0]) + word.Substring(1).ToLower();
            }
        }
        
        return result;
    }

    private bool HasProperty<T>(string propertyName)
    {
        return typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance) != null;
    }

    private LottoDraw? ParseLottoDrawFromValues(string[] values, Dictionary<string, int> propertyMappings, int lineNumber)
    {
        var lottoDraw = new LottoDraw();
        var properties = typeof(LottoDraw).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties)
        {
            if (!propertyMappings.ContainsKey(property.Name))
                continue;
                
            var columnIndex = propertyMappings[property.Name];
            if (columnIndex >= values.Length)
                continue;
                
            var value = values[columnIndex];
            
            try
            {
                SetPropertyValue(lottoDraw, property, value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set property {PropertyName} with value '{Value}' on line {LineNumber}", 
                    property.Name, value, lineNumber);
                
                // For required fields, return null to skip this record
                if (IsRequiredProperty(property))
                {
                    return null;
                }
            }
        }
        
        // Validate required fields are set
        if (!ValidateRequiredFields(lottoDraw))
        {
            _logger.LogWarning("Missing required fields on line {LineNumber}", lineNumber);
            return null;
        }
        
        return lottoDraw;
    }

    private void SetPropertyValue(LottoDraw lottoDraw, PropertyInfo property, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            // Only set null for nullable properties
            if (IsNullableProperty(property))
            {
                property.SetValue(lottoDraw, null);
            }
            return;
        }

        var propertyType = property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        object? convertedValue = underlyingType.Name switch
        {
            nameof(Int32) => int.Parse(value),
            nameof(Decimal) => decimal.Parse(value, CultureInfo.InvariantCulture),
            nameof(DateTime) => ParseDateTime(value),
            nameof(String) => value,
            _ => throw new NotSupportedException($"Property type {underlyingType.Name} is not supported")
        };

        property.SetValue(lottoDraw, convertedValue);
    }

    private DateTime ParseDateTime(string value)
    {
        // Try multiple date formats commonly used in CSV files
        var formats = new[]
        {
            "dd/MM/yyyy",
            "MM/dd/yyyy", 
            "yyyy-MM-dd",
            "dd-MM-yyyy",
            "MM-dd-yyyy",
            "dd/MM/yyyy HH:mm:ss",
            "MM/dd/yyyy HH:mm:ss"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
        }

        // Fallback to general parsing
        if (DateTime.TryParse(value, out var fallbackResult))
        {
            return fallbackResult;
        }

        throw new FormatException($"Unable to parse date: {value}");
    }

    private bool IsNullableProperty(PropertyInfo property)
    {
        return Nullable.GetUnderlyingType(property.PropertyType) != null || 
               !property.PropertyType.IsValueType;
    }

    private bool IsRequiredProperty(PropertyInfo property)
    {
        return property.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null;
    }

    private bool ValidateRequiredFields(LottoDraw lottoDraw)
    {
        // Check essential fields that must be present
        return lottoDraw.Draw > 0 && 
               lottoDraw.Date != default &&
               lottoDraw.WinningNumber1 > 0 &&
               lottoDraw.WinningNumber2 > 0 &&
               lottoDraw.WinningNumber3 > 0 &&
               lottoDraw.WinningNumber4 > 0 &&
               lottoDraw.WinningNumber5 > 0 &&
               lottoDraw.WinningNumber6 > 0 &&
               lottoDraw.BonusNumber > 0 &&
               lottoDraw.Powerball > 0;
    }
}