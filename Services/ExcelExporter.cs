using ClosedXML.Excel;
using PartTracker.Models;
using Serilog;

namespace PartTracker.Services
{
    public class ExcelExporter
    {
        private readonly ILogger _logger;

        public ExcelExporter()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.File("logs/export.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public async Task<bool> ExportRemovedPartsAsync(string outputPath, List<RemovedPart> parts)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var sheet = workbook.Worksheets.Add("Demontáž");

                    // Headers
                    int col = 1;
                    sheet.Cell(1, col++).Value = "Kód";
                    sheet.Cell(1, col++).Value = "Název";
                    sheet.Cell(1, col++).Value = "Popis";
                    sheet.Cell(1, col++).Value = "Demontován";
                    sheet.Cell(1, col++).Value = "Mechanik";
                    sheet.Cell(1, col++).Value = "Poznámky";

                    // Header styling
                    var headerRow = sheet.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.Gray;
                    headerRow.Style.Font.FontColor = XLColor.White;

                    // Data
                    int row = 2;
                    foreach (var part in parts)
                    {
                        col = 1;
                        sheet.Cell(row, col++).Value = part.PartCode;
                        sheet.Cell(row, col++).Value = part.PartName ?? "N/A";
                        sheet.Cell(row, col++).Value = part.PartDescription ?? "";
                        sheet.Cell(row, col++).Value = DateTime.Parse(part.RemovedAt).ToString("yyyy-MM-dd HH:mm:ss");
                        sheet.Cell(row, col++).Value = part.MechanikName ?? "N/A";
                        sheet.Cell(row, col++).Value = part.AdminNotes ?? "";
                        row++;
                    }

                    // Auto-fit columns
                    sheet.Columns().AdjustToContents();

                    workbook.SaveAs(outputPath);
                }

                _logger.Information($"Export completed: {parts.Count} parts exported to {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Export failed: {ex.Message}");
                return false;
            }
        }
    }
}