using System.Security.Cryptography;
using PartTracker.Models;
using Serilog;

namespace PartTracker.Services
{
    public class AuthService
    {
        private readonly DatabaseService _db;
        private readonly ILogger _logger;

        public AuthService(DatabaseService db)
        {
            _db = db;
            _logger = new LoggerConfiguration()
                .WriteTo.File("logs/auth.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public async Task<(bool success, User? user)> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _db.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    _logger.Warning($"Login failed: user {username} not found");
                    return (false, null);
                }

                bool isValid = VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
                if (!isValid)
                {
                    _logger.Warning($"Login failed: incorrect password for {username}");
                    return (false, null);
                }

                _logger.Information($"User {username} ({user.Role}) logged in successfully");
                return (true, user);
            }
            catch (Exception ex)
            {
                _logger.Error($"Login error: {ex.Message}");
                return (false, null);
            }
        }

        public async Task<bool> CreateUserAsync(string username, string password, string role)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    return false;
                }

                var (hash, salt) = HashPassword(password);
                bool success = await _db.CreateUserAsync(username, hash, salt, role);
                
                if (success)
                {
                    _logger.Information($"User {username} created with role {role}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating user: {ex.Message}");
                return false;
            }
        }

        private (string hash, string salt) HashPassword(string password)
        {
            using (var rfc2898 = new Rfc2898DeriveBytes(password, 16, 10000, HashAlgorithmName.SHA256))
            {
                string hash = Convert.ToBase64String(rfc2898.GetBytes(20));
                string salt = Convert.ToBase64String(rfc2898.Salt);
                return (hash, salt);
            }
        }

        private bool VerifyPassword(string password, string hash, string salt)
        {
            try
            {
                var saltBytes = Convert.FromBase64String(salt);
                using (var rfc2898 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256))
                {
                    string newHash = Convert.ToBase64String(rfc2898.GetBytes(20));
                    return newHash == hash;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}