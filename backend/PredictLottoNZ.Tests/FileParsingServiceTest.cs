using Microsoft.Extensions.Logging;
using PredictLottoNZ.Services;
using System.Text;

namespace PredictLottoNZ.Tests;

public class FileParsingServiceTest
{
    private readonly IFileParsingService _fileParsingService;
    private readonly ILogger<FileParsingService> _logger;

    public FileParsingServiceTest()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<FileParsingService>();
        _fileParsingService = new FileParsingService(_logger);
    }

    public async Task RunAllTests()
    {
        Console.WriteLine("Running File Parsing Service Tests...");
        Console.WriteLine("====================================");

        await TestCsvParsing();
        await TestTxtParsing();
        await TestFileTypeValidation();

        Console.WriteLine("====================================");
        Console.WriteLine("File Parsing Service tests PASSED!");
    }

    private async Task TestCsvParsing()
    {
        Console.WriteLine("Test 1: CSV parsing with valid combinations...");

        var csvContent = @"1,2,3,4,5,6
7,8,9,10,11,12
13,14,15,16,17,18";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var combinations = await _fileParsingService.ParseFileAsync(stream, "test.csv");

        var combinationList = combinations.ToList();
        
        if (combinationList.Count != 3)
            throw new Exception($"Expected 3 combinations, got {combinationList.Count}");

        // Verify first combination
        var first = combinationList[0];
        if (!first.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }))
            throw new Exception("First combination doesn't match expected values");

        Console.WriteLine("✓ PASSED: CSV parsing with valid combinations");
    }

    private async Task TestTxtParsing()
    {
        Console.WriteLine("Test 2: TXT parsing with line-based numbers...");

        var txtContent = @"1
2
3
4
5
6
7
8
9
10
11
12";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(txtContent));
        var combinations = await _fileParsingService.ParseFileAsync(stream, "test.txt");

        var combinationList = combinations.ToList();
        
        if (combinationList.Count != 2)
            throw new Exception($"Expected 2 combinations, got {combinationList.Count}");

        // Verify first combination
        var first = combinationList[0];
        if (!first.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }))
            throw new Exception("First combination doesn't match expected values");

        // Verify second combination
        var second = combinationList[1];
        if (!second.SequenceEqual(new[] { 7, 8, 9, 10, 11, 12 }))
            throw new Exception("Second combination doesn't match expected values");

        Console.WriteLine("✓ PASSED: TXT parsing with line-based numbers");
    }

    private async Task TestFileTypeValidation()
    {
        Console.WriteLine("Test 3: File type validation...");

        // Test supported file types
        if (!_fileParsingService.IsSupportedFileType("test.csv"))
            throw new Exception("CSV should be supported");

        if (!_fileParsingService.IsSupportedFileType("test.txt"))
            throw new Exception("TXT should be supported");

        if (!_fileParsingService.IsSupportedFileType("test.pdf"))
            throw new Exception("PDF should be supported");

        // Test unsupported file types
        if (_fileParsingService.IsSupportedFileType("test.doc"))
            throw new Exception("DOC should not be supported");

        if (_fileParsingService.IsSupportedFileType("test.xlsx"))
            throw new Exception("XLSX should not be supported");

        Console.WriteLine("✓ PASSED: File type validation");
    }
}