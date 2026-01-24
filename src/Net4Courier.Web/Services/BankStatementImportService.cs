using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Net4Courier.Finance.Entities;

namespace Net4Courier.Web.Services;

public interface IBankStatementImportService
{
    Task<(List<BankStatementLine> Lines, string FileHash)> ParseCsvStatement(
        string csvContent, 
        Dictionary<string, string> columnMapping,
        long importId);
}

public class BankStatementImportService : IBankStatementImportService
{
    public Task<(List<BankStatementLine> Lines, string FileHash)> ParseCsvStatement(
        string csvContent, 
        Dictionary<string, string> columnMapping,
        long importId)
    {
        var fileHash = CalculateFileHash(csvContent);
        
        var lines = new List<BankStatementLine>();
        var csvLines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        if (csvLines.Length <= 1)
        {
            return Task.FromResult((lines, fileHash));
        }

        var header = csvLines[0].Split(',');
        var columnIndices = new Dictionary<string, int>();
        
        foreach (var mapping in columnMapping)
        {
            var columnIndex = Array.FindIndex(header, h => h.Trim().Trim('"').Equals(mapping.Value, StringComparison.OrdinalIgnoreCase));
            if (columnIndex >= 0)
            {
                columnIndices[mapping.Key] = columnIndex;
            }
        }

        for (int i = 1; i < csvLines.Length; i++)
        {
            var columns = ParseCsvLine(csvLines[i]);
            
            if (columns.Length < 3) continue;

            try
            {
                var statementLine = new BankStatementLine
                {
                    BankStatementImportId = importId,
                    TransactionDate = ParseDate(GetColumnValue(columns, columnIndices, "TransactionDate")),
                    ValueDate = ParseNullableDate(GetColumnValue(columns, columnIndices, "ValueDate")),
                    Description = GetColumnValue(columns, columnIndices, "Description"),
                    ChequeNumber = GetColumnValue(columns, columnIndices, "ChequeNumber"),
                    ReferenceNumber = GetColumnValue(columns, columnIndices, "ReferenceNumber"),
                    DebitAmount = ParseDecimal(GetColumnValue(columns, columnIndices, "DebitAmount")),
                    CreditAmount = ParseDecimal(GetColumnValue(columns, columnIndices, "CreditAmount")),
                    Balance = ParseDecimal(GetColumnValue(columns, columnIndices, "Balance")),
                    LineHash = CalculateLineHash(csvLines[i])
                };

                lines.Add(statementLine);
            }
            catch
            {
                continue;
            }
        }

        return Task.FromResult((lines, fileHash));
    }

    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var current = new StringBuilder();

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString().Trim());
        return result.ToArray();
    }

    private string GetColumnValue(string[] columns, Dictionary<string, int> columnIndices, string key)
    {
        if (columnIndices.TryGetValue(key, out int index) && index < columns.Length)
        {
            return columns[index].Trim().Trim('"');
        }
        return string.Empty;
    }

    private DateTime ParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DateTime.UtcNow.Date;

        string[] formats = { 
            "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", 
            "dd-MM-yyyy", "MM-dd-yyyy", "yyyy/MM/dd",
            "dd MMM yyyy", "MMM dd yyyy", "dd-MMM-yyyy"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return DateTime.SpecifyKind(result, DateTimeKind.Utc);
            }
        }

        if (DateTime.TryParse(value, out DateTime fallbackResult))
        {
            return DateTime.SpecifyKind(fallbackResult, DateTimeKind.Utc);
        }

        return DateTime.UtcNow.Date;
    }

    private DateTime? ParseNullableDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return ParseDate(value);
    }

    private decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        value = value.Replace("$", "").Replace("£", "").Replace("€", "")
                    .Replace("AED", "").Replace("SAR", "").Replace(",", "").Trim();
        
        if (value.StartsWith("(") && value.EndsWith(")"))
        {
            value = "-" + value.Trim('(', ')');
        }

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
        {
            return result;
        }

        return 0;
    }

    private string CalculateFileHash(string content)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hashBytes);
    }

    private string CalculateLineHash(string line)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(line));
        return Convert.ToHexString(hashBytes).Substring(0, Math.Min(64, Convert.ToHexString(hashBytes).Length));
    }
}
