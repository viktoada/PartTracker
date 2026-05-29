using System.Data;
using Dapper;
using System.Data.SQLite;
using PartTracker.Models;
using Serilog;

namespace PartTracker.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        public DatabaseService(string dbPath = "tracker.db")
        {
            _connectionString = $"Data Source={dbPath};Version=3;";
            _logger = new LoggerConfiguration()
                .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public async Task InitializeAsync()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Parts table
                    await connection.ExecuteAsync(@"
                        CREATE TABLE IF NOT EXISTS Parts (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Code TEXT UNIQUE NOT NULL,
                            Name TEXT NOT NULL,
                            Description TEXT,
                            Metadata TEXT,
                            ImportDate TEXT NOT NULL
                        );
                        CREATE INDEX IF NOT EXISTS idx_code ON Parts(Code);
                    "
                    );

                    // Users table
                    await connection.ExecuteAsync(@"
                        CREATE TABLE IF NOT EXISTS Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username TEXT UNIQUE NOT NULL,
                            PasswordHash TEXT NOT NULL,
                            PasswordSalt TEXT NOT NULL,
                            Role TEXT NOT NULL,
                            CreatedAt TEXT NOT NULL
                        );
                    "
                    );

                    // PrintLog table
                    await connection.ExecuteAsync(@"
                        CREATE TABLE IF NOT EXISTS PrintLog (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            PartCode TEXT NOT NULL,
                            UserId INTEGER NOT NULL,
                            PrintedAt TEXT NOT NULL,
                            Status TEXT NOT NULL,
                            ErrorMessage TEXT,
                            FOREIGN KEY (UserId) REFERENCES Users(Id)
                        );
                    "
                    );

                    // RemovedParts table
                    await connection.ExecuteAsync(@"
                        CREATE TABLE IF NOT EXISTS RemovedParts (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            PartCode TEXT NOT NULL,
                            UserId INTEGER NOT NULL,
                            RemovedAt TEXT NOT NULL,
                            AdminNotes TEXT,
                            FOREIGN KEY (UserId) REFERENCES Users(Id),
                            FOREIGN KEY (PartCode) REFERENCES Parts(Code)
                        );
                    "
                    );

                    _logger.Information("Database initialized successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Database initialization failed: {ex.Message}");
                throw;
            }
        }

        public async Task<Part?> GetPartByCodeAsync(string code)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QueryFirstOrDefaultAsync<Part>(
                        "SELECT * FROM Parts WHERE Code = @code LIMIT 1",
                        new { code }
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching part by code: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Part>> GetSimilarPartsAsync(string codePattern, int limit = 10)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var results = await connection.QueryAsync<Part>(
                        "SELECT * FROM Parts WHERE Code LIKE @pattern LIMIT @limit",
                        new { pattern = codePattern + "%", limit }
                    );
                    return results.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching similar parts: {ex.Message}");
                return new List<Part>();
            }
        }

        public async Task<int> InsertPartsAsync(List<Part> parts)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int inserted = 0;

                    foreach (var part in parts)
                    {
                        try
                        {
                            part.ImportDate = DateTime.Now;
                            await connection.ExecuteAsync(
                                @"INSERT OR REPLACE INTO Parts (Code, Name, Description, Metadata, ImportDate) 
                                  VALUES (@Code, @Name, @Description, @Metadata, @ImportDate)",
                                part
                            );
                            inserted++;
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning($"Duplicate or invalid part {part.Code}: {ex.Message}");
                        }
                    }

                    _logger.Information($"Inserted {inserted} parts out of {parts.Count}");
                    return inserted;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error inserting parts: {ex.Message}");
                return 0;
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await connection.QueryFirstOrDefaultAsync<User>(
                        "SELECT * FROM Users WHERE Username = @username",
                        new { username }
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching user: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(string username, string passwordHash, string salt, string role)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(
                        @"INSERT INTO Users (Username, PasswordHash, PasswordSalt, Role, CreatedAt) 
                          VALUES (@username, @hash, @salt, @role, @created)",
                        new { username, hash = passwordHash, salt, role, created = DateTime.Now.ToString("o") }
                    );
                    _logger.Information($"User {username} created with role {role}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating user: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LogPrintAsync(string partCode, int userId, string status, string? errorMessage = null)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(
                        @"INSERT INTO PrintLog (PartCode, UserId, PrintedAt, Status, ErrorMessage) 
                          VALUES (@partCode, @userId, @printedAt, @status, @errorMessage)",
                        new { partCode, userId, printedAt = DateTime.Now.ToString("o"), status, errorMessage }
                    );
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error logging print: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LogRemovedPartAsync(string partCode, int userId, string? adminNotes = null)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync(
                        @"INSERT INTO RemovedParts (PartCode, UserId, RemovedAt, AdminNotes) 
                          VALUES (@partCode, @userId, @removedAt, @adminNotes)",
                        new { partCode, userId, removedAt = DateTime.Now.ToString("o"), adminNotes }
                    );
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error logging removed part: {ex.Message}");
                return false;
            }
        }

        public async Task<List<PrintLog>> GetPrintLogsAsync(int limit = 100)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var results = await connection.QueryAsync<PrintLog>(
                        "SELECT * FROM PrintLog ORDER BY PrintedAt DESC LIMIT @limit",
                        new { limit }
                    );
                    return results.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching print logs: {ex.Message}");
                return new List<PrintLog>();
            }
        }

        public async Task<List<RemovedPart>> GetRemovedPartsAsync()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var results = await connection.QueryAsync<RemovedPart>(
                        @"SELECT rp.*, p.Name as PartName, p.Description as PartDescription, u.Username as MechanikName
                          FROM RemovedParts rp
                          LEFT JOIN Parts p ON rp.PartCode = p.Code
                          LEFT JOIN Users u ON rp.UserId = u.Id
                          ORDER BY rp.RemovedAt DESC"
                    );
                    return results.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error fetching removed parts: {ex.Message}");
                return new List<RemovedPart>();
            }
        }
    }
}