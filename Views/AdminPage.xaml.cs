using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PartTracker.Services;
using Windows.Storage.Pickers;

namespace PartTracker.Views
{
    public sealed partial class AdminPage : Page
    {
        private Action _onLogout;
        private ImportService _importService;
        private ExportService _exportService;

        public AdminPage(Action onLogout)
        {
            this.InitializeComponent();
            _onLogout = onLogout;
            _importService = new ImportService();
            _exportService = new ExportService();
        }

        private async void OnSelectExcelClick(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".xlsx");
            picker.FileTypeFilter.Add(".xls");
            
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                ImportStatus.Text = $"Importuji: {file.Name}...";
                ImportProgress.IsActive = true;

                try
                {
                    int imported = await _importService.ImportPartsFromExcelAsync(file);
                    ImportStatus.Text = $"✓ Úspěšně importováno {imported} dílů.";
                }
                catch (Exception ex)
                {
                    ImportStatus.Text = $"✗ Chyba: {ex.Message}";
                }
                finally
                {
                    ImportProgress.IsActive = false;
                }
            }
        }

        private async void OnExportClick(object sender, RoutedEventArgs e)
        {
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("Excel soubor", new List<string> { ".xlsx" });
            picker.SuggestedFileName = $"export_demontaza_{DateTime.Now:yyyyMMdd_HHmmss}";

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                ExportStatus.Text = "Exportuji...";

                try
                {
                    await _exportService.ExportDemontageToExcelAsync(file);
                    ExportStatus.Text = "✓ Export úspěšně uložen.";
                }
                catch (Exception ex)
                {
                    ExportStatus.Text = $"✗ Chyba: {ex.Message}";
                }
            }
        }

        private async void OnRefreshLogsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var logs = await _exportService.GetPrintLogsAsync();
                PrintLogsList.ItemsSource = logs;
            }
            catch (Exception ex)
            {
                var errorText = new TextBlock { Text = $"Chyba: {ex.Message}" };
                PrintLogsList.Items.Add(errorText);
            }
        }

        private async void OnCreateUserClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateUserDialog();
            var result = await dialog.ShowAsync();
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            _onLogout?.Invoke();
        }
    }
}