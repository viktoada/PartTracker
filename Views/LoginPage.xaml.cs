using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PartTracker.Services;

namespace PartTracker.Views
{
    public sealed partial class LoginPage : Page
    {
        private AuthenticationService _authService;
        private Action<string> _onLoginSuccess;

        public LoginPage(Action<string> onLoginSuccess)
        {
            this.InitializeComponent();
            _authService = new AuthenticationService();
            _onLoginSuccess = onLoginSuccess;
        }

        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            string userId = UsernameInput.Text;
            string password = PasswordInput.Password;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                ErrorMessage.Text = "Vyplňte prosím všechna pole.";
                return;
            }

            LoadingRing.IsActive = true;
            LoginButton.IsEnabled = false;

            try
            {
                bool success = await _authService.AuthenticateAsync(userId, password);
                if (success)
                {
                    _onLoginSuccess?.Invoke(userId);
                }
                else
                {
                    ErrorMessage.Text = "Nesprávné ID nebo heslo.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Chyba: {ex.Message}";
            }
            finally
            {
                LoadingRing.IsActive = false;
                LoginButton.IsEnabled = true;
            }
        }
    }
}