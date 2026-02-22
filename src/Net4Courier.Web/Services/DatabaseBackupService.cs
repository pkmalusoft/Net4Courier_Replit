using System.Collections.Concurrent;
using System.Diagnostics;

namespace Net4Courier.Web.Services;

public class DatabaseBackupService
{
    private readonly ILogger<DatabaseBackupService> _logger;
    private static readonly SemaphoreSlim _backupLock = new(1, 1);
    private static readonly ConcurrentDictionary<string, BackupToken> _tokens = new();

    public DatabaseBackupService(ILogger<DatabaseBackupService> logger)
    {
        _logger = logger;
    }

    public record BackupResult(bool Success, string? DownloadToken, string? FileName, long FileSizeBytes, string? ErrorMessage);

    private record BackupToken(string FilePath, string FileName, long UserId, DateTime ExpiresAt);

    public async Task<BackupResult> CreateBackupAsync(long userId, bool dataOnly = false, CancellationToken cancellationToken = default)
    {
        if (!await _backupLock.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken))
            return new BackupResult(false, null, null, 0, "Another backup is already in progress. Please wait.");

        try
        {
            CleanupExpiredTokens();

            var (host, port, database, username, password) = ParseConnectionString();
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(database))
                return new BackupResult(false, null, null, 0, "Database connection is not configured.");

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var suffix = dataOnly ? "data" : "full";
            var fileName = $"net4courier_backup_{suffix}_{timestamp}.sql";
            var tempDir = Path.Combine(Path.GetTempPath(), "net4courier_backups");
            Directory.CreateDirectory(tempDir);
            var filePath = Path.Combine(tempDir, fileName);

            var pgDumpPath = FindPgDump();
            if (string.IsNullOrEmpty(pgDumpPath))
                return new BackupResult(false, null, null, 0, "pg_dump utility not found on this system.");

            var connUri = $"postgresql://{Uri.EscapeDataString(username)}:{Uri.EscapeDataString(password)}@{host}:{port}/{database}";
            var args = $"--dbname=\"{connUri}\" --no-password --format=plain --encoding=UTF8";
            if (dataOnly)
                args += " --data-only";
            else
                args += " --clean --if-exists --create";
            args += $" --file=\"{filePath}\"";

            var psi = new ProcessStartInfo
            {
                FileName = pgDumpPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _logger.LogInformation("Starting database backup: {FileName} (dataOnly: {DataOnly})", fileName, dataOnly);

            using var process = new Process { StartInfo = psi };
            process.Start();

            var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                _logger.LogError("pg_dump failed with exit code {ExitCode}: {Error}", process.ExitCode, stderr);
                if (File.Exists(filePath)) File.Delete(filePath);
                return new BackupResult(false, null, null, 0, $"Backup failed: {stderr}");
            }

            var fileInfo = new FileInfo(filePath);
            _logger.LogInformation("Backup completed: {FileName} ({Size} bytes)", fileName, fileInfo.Length);

            var token = Guid.NewGuid().ToString("N");
            _tokens[token] = new BackupToken(filePath, fileName, userId, DateTime.UtcNow.AddMinutes(10));

            return new BackupResult(true, token, fileName, fileInfo.Length, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating database backup");
            return new BackupResult(false, null, null, 0, $"Backup error: {ex.Message}");
        }
        finally
        {
            _backupLock.Release();
        }
    }

    public (bool Valid, string? FilePath, string? FileName) ValidateAndConsumeToken(string token)
    {
        if (!_tokens.TryRemove(token, out var backupToken))
            return (false, null, null);

        if (DateTime.UtcNow > backupToken.ExpiresAt)
        {
            CleanupBackupFile(backupToken.FilePath);
            return (false, null, null);
        }

        if (!File.Exists(backupToken.FilePath))
            return (false, null, null);

        return (true, backupToken.FilePath, backupToken.FileName);
    }

    public async Task<(string DbName, string DbHost, long TableCount, long EstimatedSizeMB)> GetDatabaseInfoAsync()
    {
        var (host, port, database, username, password) = ParseConnectionString();
        if (string.IsNullOrEmpty(host))
            return ("N/A", "N/A", 0, 0);

        try
        {
            using var conn = new Npgsql.NpgsqlConnection(
                $"Host={host};Port={port};Database={database};Username={username};Password={password};Timeout=10");
            await conn.OpenAsync();

            long tableCount = 0;
            long sizeBytes = 0;

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE'";
                tableCount = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT pg_database_size(current_database())";
                sizeBytes = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            }

            return (database, host, tableCount, sizeBytes / (1024 * 1024));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database info");
            return (database ?? "N/A", host ?? "N/A", 0, 0);
        }
    }

    public void CleanupBackupFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup backup file: {Path}", filePath);
        }
    }

    private void CleanupExpiredTokens()
    {
        var expired = _tokens.Where(kv => DateTime.UtcNow > kv.Value.ExpiresAt).ToList();
        foreach (var kv in expired)
        {
            if (_tokens.TryRemove(kv.Key, out var token))
                CleanupBackupFile(token.FilePath);
        }
    }

    private (string Host, string Port, string Database, string Username, string Password) ParseConnectionString()
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrEmpty(databaseUrl))
            return (string.Empty, "5432", string.Empty, string.Empty, string.Empty);

        try
        {
            if (databaseUrl.StartsWith("postgresql://") || databaseUrl.StartsWith("postgres://"))
            {
                var uri = new Uri(databaseUrl);
                var userInfo = uri.UserInfo.Split(':');
                return (
                    uri.Host,
                    (uri.Port > 0 ? uri.Port : 5432).ToString(),
                    uri.AbsolutePath.TrimStart('/').Split('?')[0],
                    Uri.UnescapeDataString(userInfo[0]),
                    userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : ""
                );
            }

            var parts = databaseUrl.Split(';')
                .Select(p => p.Split('=', 2))
                .Where(p => p.Length == 2)
                .ToDictionary(p => p[0].Trim(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);

            return (
                parts.GetValueOrDefault("Host", ""),
                parts.GetValueOrDefault("Port", "5432"),
                parts.GetValueOrDefault("Database", ""),
                parts.GetValueOrDefault("Username", ""),
                parts.GetValueOrDefault("Password", "")
            );
        }
        catch
        {
            return (string.Empty, "5432", string.Empty, string.Empty, string.Empty);
        }
    }

    private string? FindPgDump()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "pg_dump",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) return null;
            var path = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return process.ExitCode == 0 && !string.IsNullOrEmpty(path) ? path : null;
        }
        catch
        {
            return null;
        }
    }
}
