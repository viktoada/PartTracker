using Microsoft.UI.Xaml;
using PartTracker.Services;
using PartTracker.Views;

namespace PartTracker
{
    public sealed partial class MainWindow : Window
    {
        private AuthenticationService _authService;

        public MainWindow()
        {
            this.InitializeComponent();
            _authService = new AuthenticationService();
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Initialize the application
            await LoadLoginPageAsync();
        }

        private async Task LoadLoginPageAsync()
        {
            var loginPage = new LoginPage(OnLoginSuccess);
            this.Content = loginPage;
        }

        private void OnLoginSuccess(string userId)
        {
            var searchPage = new SearchPage(userId, OnLogout);
            this.Content = searchPage;
        }

        private void OnLogout()
        {
            _ = LoadLoginPageAsync();
        }
    }
}