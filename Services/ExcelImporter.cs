using ClosedXML.Excel;
using PartTracker.Models;
using Serilog;

namespace PartTracker.Services
{
    public class ExcelImporter
    {
        private readonly ILogger _logger;

        public ExcelImporter()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.File("logs/import.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public async Task<ImportResult> ImportAsync(string filePath)
        {
            var result = new ImportResult();

            try
            {
                if (!File.Exists(filePath))
                {
                    result.Errors.Add($"File not found: {filePath}");
                    return result;
                }

                using (var workbook = new XLWorkbook(filePath))
                {
                    if (workbook.Worksheets.Count == 0)
                    {
                        result.Errors.Add("Excel file has no worksheets");
                        return result;
                    }

                    var sheet = workbook.Worksheets.First();
                    var rows = sheet.RowsUsed().ToList();

                    if (rows.Count < 2)
                    {
                        result.Errors.Add("Excel file must have at least 2 rows (header + data)");
                        return result;
                    }

                    // Skip header row
                    foreach (var row in rows.Skip(1))
                    {
                        try
                        {
                            var codeCell = row.Cell(1);
                            var nameCell = row.Cell(2);
                            var descCell = row.Cell(3);

                            var code = codeCell?.Value?.ToString()?.Trim();
                            var name = nameCell?.Value?.ToString()?.Trim();
                            var desc = descCell?.Value?.ToString()?.Trim();

                            // Validation
                            if (string.IsNullOrEmpty(code) || code.Length != 5 || !code.All(char.IsDigit))
                            {
                                result.Errors.Add($"Row {row.RowNumber}: Invalid code '{code}' (must be 5 digits)");
                                continue;
                            }

                            if (string.IsNullOrEmpty(name))
                            {
                                result.Errors.Add($"Row {row.RowNumber}: Missing part name for code {code}");
                                continue;
                            }

                            var part = new Part
                            {
                                Code = code,
                                Name = name,
                                Description = desc ?? "",
                                ImportDate = DateTime.Now
                            };

                            result.Parts.Add(part);
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Row {row.RowNumber}: {ex.Message}");
                        }
                    }
                }

                _logger.Information($"Import completed: {result.Parts.Count} parts, {result.Errors.Count} errors");
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Import failed: {ex.Message}");
                _logger.Error($"Import error: {ex.Message}");
            }

            return result;
        }
    }

    public class ImportResult
    {
        public List<Part> Parts { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}