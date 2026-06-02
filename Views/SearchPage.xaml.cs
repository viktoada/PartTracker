using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PartTracker.Services;
using PartTracker.Models;

namespace PartTracker.Views
{
    public sealed partial class SearchPage : Page
    {
        private string _userId;
        private Action _onLogout;
        private PartService _partService;
        private PrintService _printService;
        private Part _selectedPart;

        public SearchPage(string userId, Action onLogout)
        {
            this.InitializeComponent();
            _userId = userId;
            _onLogout = onLogout;
            _partService = new PartService();
            _printService = new PrintService();
            
            UserInfo.Text = $"Přihlášen: {userId}";
        }

        private async void OnSearchClick(object sender, RoutedEventArgs e)
        {
            await PerformSearch();
        }

        private async void OnCodeInputKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await PerformSearch();
            }
        }

        private async Task PerformSearch()
        {
            string code = CodeInput.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                return;
            }

            ResultPanel.Children.Clear();
            PrintButton.IsEnabled = false;

            try
            {
                _selectedPart = await _partService.SearchPartAsync(code);
                if (_selectedPart != null)
                {
                    DisplayPartInfo(_selectedPart);
                    PrintButton.IsEnabled = true;
                }
                else
                {
                    var errorText = new TextBlock
                    {
                        Text = $"Díl s kódem {code} nebyl nalezen.",
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 107, 107))
                    };
                    ResultPanel.Children.Add(errorText);
                }
            }
            catch (Exception ex)
            {
                var errorText = new TextBlock
                {
                    Text = $"Chyba: {ex.Message}",
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 107, 107))
                };
                ResultPanel.Children.Add(errorText);
            }
        }

        private void DisplayPartInfo(Part part)
        {
            var codeText = new TextBlock
            {
                Text = $"Kód: {part.Code}",
                FontSize = 16,
                FontWeight = Windows.UI.Text.FontWeights.Bold
            };
            ResultPanel.Children.Add(codeText);

            var nameText = new TextBlock
            {
                Text = $"Název: {part.Name}",
                FontSize = 14,
                Margin = new Thickness(0, 10, 0, 0)
            };
            ResultPanel.Children.Add(nameText);

            var descText = new TextBlock
            {
                Text = $"Popis: {part.Description}",
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 10, 0, 0)
            };
            ResultPanel.Children.Add(descText);
        }

        private async void OnPrintClick(object sender, RoutedEventArgs e)
        {
            if (_selectedPart != null)
            {
                try
                {
                    await _printService.PrintPartLabelAsync(_selectedPart, _userId);
                    var successText = new TextBlock
                    {
                        Text = "Štítek byl úspěšně vytisknut.",
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80))
                    };
                    ResultPanel.Children.Add(successText);
                }
                catch (Exception ex)
                {
                    var errorText = new TextBlock
                    {
                        Text = $"Chyba tisku: {ex.Message}",
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 107, 107))
                    };
                    ResultPanel.Children.Add(errorText);
                }
            }
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            _onLogout?.Invoke();
        }
    }
}