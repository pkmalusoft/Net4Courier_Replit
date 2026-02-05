using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text;

namespace Net4Courier.Infrastructure.Services;

public class SchemaAutoSyncService
{
    private readonly ILogger<SchemaAutoSyncService> _logger;

    public SchemaAutoSyncService(ILogger<SchemaAutoSyncService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SyncSchemaAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting automatic schema synchronization...");

            var model = dbContext.Model;
            var connection = dbContext.Database.GetDbConnection();
            
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            var existingTables = await GetExistingTablesAsync(connection, cancellationToken);
            var existingColumns = await GetExistingColumnsAsync(connection, cancellationToken);

            var sqlStatements = new List<string>();

            foreach (var entityType in model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (string.IsNullOrEmpty(tableName)) continue;

                if (!existingTables.Contains(tableName))
                {
                    var createTableSql = GenerateCreateTableStatement(entityType, tableName);
                    sqlStatements.Add(createTableSql);
                    _logger.LogInformation("Will create table: {TableName}", tableName);
                }
                else
                {
                    var tableColumns = existingColumns
                        .Where(c => c.TableName == tableName)
                        .Select(c => c.ColumnName)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    foreach (var property in entityType.GetProperties())
                    {
                        var columnName = property.GetColumnName();
                        if (string.IsNullOrEmpty(columnName)) continue;

                        if (!tableColumns.Contains(columnName))
                        {
                            var alterSql = GenerateAddColumnStatement(tableName, property);
                            sqlStatements.Add(alterSql);
                            _logger.LogInformation("Will add column: {TableName}.{ColumnName}", tableName, columnName);
                        }
                    }
                }
            }

            if (sqlStatements.Count == 0)
            {
                _logger.LogInformation("Schema is already up to date. No changes needed.");
                return true;
            }

            _logger.LogInformation("Executing {Count} schema changes...", sqlStatements.Count);

            foreach (var sql in sqlStatements)
            {
                try
                {
                    await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
                    _logger.LogDebug("Executed: {Sql}", sql.Length > 200 ? sql[..200] + "..." : sql);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to execute schema change: {Sql}", sql.Length > 200 ? sql[..200] + "..." : sql);
                }
            }

            _logger.LogInformation("Schema synchronization completed successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schema auto-sync failed.");
            return false;
        }
    }

    public string GenerateFullSchemaScript(DbContext dbContext)
    {
        var sb = new StringBuilder();
        sb.AppendLine("-- Net4Courier Auto-Generated Schema Sync Script");
        sb.AppendLine($"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine("-- This script uses IF NOT EXISTS for safe re-execution");
        sb.AppendLine();
        sb.AppendLine("-- =============================================");
        sb.AppendLine("-- TABLES");
        sb.AppendLine("-- =============================================");
        sb.AppendLine();

        var model = dbContext.Model;
        var processedTables = new HashSet<string>();

        foreach (var entityType in model.GetEntityTypes().OrderBy(e => e.GetTableName()))
        {
            var tableName = entityType.GetTableName();
            if (string.IsNullOrEmpty(tableName) || processedTables.Contains(tableName)) continue;
            processedTables.Add(tableName);

            var createTableSql = GenerateCreateTableStatement(entityType, tableName);
            sb.AppendLine($"-- Table: {tableName}");
            sb.AppendLine(createTableSql);
            sb.AppendLine();

            var indexes = GenerateIndexStatements(entityType, tableName);
            if (!string.IsNullOrEmpty(indexes))
            {
                sb.AppendLine(indexes);
                sb.AppendLine();
            }
        }

        sb.AppendLine("-- =============================================");
        sb.AppendLine("-- END OF SCHEMA SYNC SCRIPT");
        sb.AppendLine("-- =============================================");

        return sb.ToString();
    }

    private string GenerateCreateTableStatement(IEntityType entityType, string tableName)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE IF NOT EXISTS \"{tableName}\" (");

        var properties = entityType.GetProperties().ToList();
        var columnDefs = new List<string>();

        foreach (var property in properties)
        {
            var columnName = property.GetColumnName();
            if (string.IsNullOrEmpty(columnName)) continue;

            var columnType = GetPostgresType(property);
            var isNullable = property.IsNullable;
            var isPrimaryKey = property.IsPrimaryKey();

            var columnDef = $"    \"{columnName}\" {columnType}";

            if (isPrimaryKey && columnType == "BIGINT")
            {
                columnDef = $"    \"{columnName}\" BIGSERIAL PRIMARY KEY";
            }
            else if (isPrimaryKey && columnType == "INT")
            {
                columnDef = $"    \"{columnName}\" SERIAL PRIMARY KEY";
            }
            else
            {
                if (!isNullable && !isPrimaryKey)
                {
                    columnDef += " NOT NULL";
                }

                var defaultValue = GetDefaultValue(property);
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    columnDef += $" DEFAULT {defaultValue}";
                }
            }

            columnDefs.Add(columnDef);
        }

        sb.AppendLine(string.Join(",\n", columnDefs));
        sb.AppendLine(");");

        return sb.ToString();
    }

    private string GenerateAddColumnStatement(string tableName, IProperty property)
    {
        var columnName = property.GetColumnName();
        var columnType = GetPostgresType(property);
        var isNullable = property.IsNullable;

        var sql = $"ALTER TABLE \"{tableName}\" ADD COLUMN IF NOT EXISTS \"{columnName}\" {columnType}";

        var defaultValue = GetDefaultValue(property);
        if (!string.IsNullOrEmpty(defaultValue))
        {
            sql += $" DEFAULT {defaultValue}";
        }

        if (!isNullable)
        {
            sql += " NOT NULL";
        }

        sql += ";";
        return sql;
    }

    private string GenerateIndexStatements(IEntityType entityType, string tableName)
    {
        var sb = new StringBuilder();
        var indexes = entityType.GetIndexes();

        foreach (var index in indexes)
        {
            var indexName = index.GetDatabaseName() ?? $"IX_{tableName}_{string.Join("_", index.Properties.Select(p => p.GetColumnName()))}";
            var columns = string.Join(", ", index.Properties.Select(p => $"\"{p.GetColumnName()}\""));
            var unique = index.IsUnique ? "UNIQUE " : "";

            sb.AppendLine($"CREATE {unique}INDEX IF NOT EXISTS \"{indexName}\" ON \"{tableName}\" ({columns});");
        }

        return sb.ToString();
    }

    private string GetPostgresType(IProperty property)
    {
        var clrType = property.ClrType;
        var underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;

        if (underlyingType == typeof(long)) return "BIGINT";
        if (underlyingType == typeof(int)) return "INT";
        if (underlyingType == typeof(short)) return "SMALLINT";
        if (underlyingType == typeof(byte)) return "SMALLINT";
        if (underlyingType == typeof(bool)) return "BOOLEAN";
        if (underlyingType == typeof(decimal)) return "DECIMAL(18,2)";
        if (underlyingType == typeof(double)) return "DOUBLE PRECISION";
        if (underlyingType == typeof(float)) return "REAL";
        if (underlyingType == typeof(DateTime)) return "TIMESTAMP WITH TIME ZONE";
        if (underlyingType == typeof(DateTimeOffset)) return "TIMESTAMP WITH TIME ZONE";
        if (underlyingType == typeof(DateOnly)) return "DATE";
        if (underlyingType == typeof(TimeOnly)) return "TIME";
        if (underlyingType == typeof(TimeSpan)) return "INTERVAL";
        if (underlyingType == typeof(Guid)) return "UUID";
        if (underlyingType == typeof(byte[])) return "BYTEA";

        if (underlyingType == typeof(string))
        {
            var maxLength = property.GetMaxLength();
            return maxLength.HasValue ? $"VARCHAR({maxLength.Value})" : "TEXT";
        }

        if (underlyingType.IsEnum) return "INT";

        return "TEXT";
    }

    private string GetDefaultValue(IProperty property)
    {
        var clrType = property.ClrType;
        var underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;

        if (underlyingType == typeof(bool)) return "FALSE";
        if (underlyingType == typeof(int) || underlyingType == typeof(long) || underlyingType == typeof(short)) return "0";
        if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float)) return "0";
        if (underlyingType == typeof(DateTime)) return "CURRENT_TIMESTAMP";

        if (property.Name == "IsActive") return "TRUE";
        if (property.Name == "IsDeleted") return "FALSE";
        if (property.Name == "IsDemo") return "FALSE";
        if (property.Name == "CreatedAt") return "CURRENT_TIMESTAMP";

        return null!;
    }

    private async Task<HashSet<string>> GetExistingTablesAsync(System.Data.Common.DbConnection connection, CancellationToken cancellationToken)
    {
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT tablename FROM pg_tables WHERE schemaname = 'public'";

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    private async Task<List<(string TableName, string ColumnName)>> GetExistingColumnsAsync(System.Data.Common.DbConnection connection, CancellationToken cancellationToken)
    {
        var columns = new List<(string TableName, string ColumnName)>();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT table_name, column_name 
            FROM information_schema.columns 
            WHERE table_schema = 'public'";

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add((reader.GetString(0), reader.GetString(1)));
        }

        return columns;
    }

    public async Task<bool> ExecuteSchemaScriptAsync(DbContext dbContext, string scriptPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(scriptPath))
            {
                _logger.LogWarning("Schema script not found: {Path}", scriptPath);
                return false;
            }

            var script = await File.ReadAllTextAsync(scriptPath, cancellationToken);
            
            // Parse statements more carefully - handle CREATE TABLE blocks correctly
            var statements = ParseSqlStatements(script)
                .Where(s => s.Contains("CREATE TABLE") || s.Contains("ALTER TABLE") || s.Contains("CREATE INDEX"))
                .ToList();

            _logger.LogInformation("Executing {Count} statements from {Path}", statements.Count, scriptPath);

            foreach (var statement in statements)
            {
                try
                {
                    await dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Statement skipped (may already exist): {Sql}", statement.Length > 100 ? statement[..100] + "..." : statement);
                }
            }

            _logger.LogInformation("Schema script execution completed.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute schema script: {Path}", scriptPath);
            return false;
        }
    }

    private List<string> ParseSqlStatements(string script)
    {
        var statements = new List<string>();
        var currentStatement = new StringBuilder();
        var inCreateTable = false;
        var parenthesesDepth = 0;

        var lines = script.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip pure comment lines and empty lines when not in a statement
            if (currentStatement.Length == 0 && (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("--")))
            {
                continue;
            }

            // Add line to current statement
            currentStatement.AppendLine(line);

            // Track CREATE TABLE blocks
            if (trimmedLine.Contains("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
            {
                inCreateTable = true;
            }

            // Count parentheses for CREATE TABLE blocks
            if (inCreateTable)
            {
                foreach (var c in trimmedLine)
                {
                    if (c == '(') parenthesesDepth++;
                    else if (c == ')') parenthesesDepth--;
                }
            }

            // Check for statement terminator
            var endsWithSemicolon = trimmedLine.EndsWith(";") || trimmedLine.EndsWith(";--");
            
            if (endsWithSemicolon)
            {
                if (inCreateTable && parenthesesDepth > 0)
                {
                    // Still inside CREATE TABLE block, continue
                    continue;
                }

                // Statement complete
                var stmt = currentStatement.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(stmt))
                {
                    statements.Add(stmt);
                }
                currentStatement.Clear();
                inCreateTable = false;
                parenthesesDepth = 0;
            }
        }

        // Handle any remaining content
        var remaining = currentStatement.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(remaining) && !remaining.StartsWith("--"))
        {
            // Add semicolon if missing
            if (!remaining.EndsWith(";"))
            {
                remaining += ";";
            }
            statements.Add(remaining);
        }

        return statements;
    }
}
