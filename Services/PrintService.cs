using System.Drawing;
using System.Drawing.Printing;
using PartTracker.Models;
using Serilog;

namespace PartTracker.Services
{
    public class PrintService
    {
        private readonly DatabaseService _db;
        private readonly ILogger _logger;
        private string? _selectedPrinterName;

        public PrintService(DatabaseService db)
        {
            _db = db;
            _logger = new LoggerConfiguration()
                .WriteTo.File("logs/print.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public List<string> GetAvailablePrinters()
        {
            return PrinterSettings.InstalledPrinters.Cast<string>().ToList();
        }

        public void SetPrinter(string printerName)
        {
            if (GetAvailablePrinters().Contains(printerName))
            {
                _selectedPrinterName = printerName;
                _logger.Information($"Printer set to: {printerName}");
            }
            else
            {
                _logger.Warning($"Printer not found: {printerName}");
            }
        }

        public async Task<bool> PrintPartLabelAsync(Part part, int userId)
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedPrinterName))
                {
                    await _db.LogPrintAsync(part.Code, userId, "Failed", "No printer selected");
                    _logger.Error("Print failed: No printer selected");
                    return false;
                }

                var printDoc = new PrintDocument { DocumentName = $"Label-{part.Code}" };
                printDoc.PrinterSettings.PrinterName = _selectedPrinterName;

                // A6 size: 105x148 mm → 413x583 points (at 96 DPI)
                var paperSize = new PaperSize("A6", 413, 583);
                printDoc.DefaultPageSettings.PaperSize = paperSize;
                printDoc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);

                // Set print event
                printDoc.PrintPage += (sender, e) => PrintLabelPage(e, part);

                printDoc.Print();

                await _db.LogPrintAsync(part.Code, userId, "Success");
                await _db.LogRemovedPartAsync(part.Code, userId);

                _logger.Information($"Part {part.Code} printed successfully by user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                await _db.LogPrintAsync(part.Code, userId, "Failed", ex.Message);
                _logger.Error($"Print failed for {part.Code}: {ex.Message}");
                return false;
            }
        }

        private void PrintLabelPage(PrintPageEventArgs e, Part part)
        {
            try
            {
                var graphics = e.Graphics;
                if (graphics == null) return;

                // Margins
                int marginLeft = 20;
                int marginTop = 20;
                int currentY = marginTop;

                // Code (large, bold)
                using (var fontCode = new Font("Arial", 28, FontStyle.Bold))
                {
                    graphics.DrawString(
                        part.Code,
                        fontCode,
                        Brushes.Black,
                        marginLeft,
                        currentY
                    );
                    currentY += 50;
                }

                // Name
                using (var fontName = new Font("Arial", 12, FontStyle.Bold))
                {
                    graphics.DrawString(
                        part.Name,
                        fontName,
                        Brushes.Black,
                        marginLeft,
                        currentY
                    );
                    currentY += 40;
                }

                // Description (wrapped)
                if (!string.IsNullOrEmpty(part.Description))
                {
                    using (var fontDesc = new Font("Arial", 9))
                    {
                        var rect = new RectangleF(marginLeft, currentY, 360, 150);
                        graphics.DrawString(
                            part.Description,
                            fontDesc,
                            Brushes.Black,
                            rect,
                            StringFormat.GenericDefault
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error drawing label: {ex.Message}");
            }
        }

        public async Task<bool> PrintMultipleAsync(List<Part> parts, int userId)
        {
            int successCount = 0;
            foreach (var part in parts)
            {
                if (await PrintPartLabelAsync(part, userId))
                    successCount++;

                // Small delay between prints
                await Task.Delay(100);
            }

            _logger.Information($"Batch print completed: {successCount}/{parts.Count} successful");
            return successCount == parts.Count;
        }
    }
}