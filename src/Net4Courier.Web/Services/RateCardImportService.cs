using ClosedXML.Excel;
using Net4Courier.Kernel.Enums;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Web.Services;

public class RateCardImportResult
{
    public List<ImportedZoneCategory> ZoneCategories { get; set; } = new();
    public List<ImportedRateCard> RateCards { get; set; } = new();
    public List<RateCardImportError> Errors { get; set; } = new();
    public bool IsValid => !Errors.Any(e => e.IsCritical);
}

public class ImportedZoneCategory
{
    public string Name { get; set; } = string.Empty;
    public List<ImportedZone> Zones { get; set; } = new();
    public long? ExistingId { get; set; }
    public bool CreateNew { get; set; } = true;
}

public class ImportedZone
{
    public string ZoneCode { get; set; } = string.Empty;
    public List<string> Countries { get; set; } = new();
    public long? ExistingZoneMatrixId { get; set; }
}

public class ImportedRateCard
{
    public string Name { get; set; } = string.Empty;
    public List<ImportedRateCardZone> Zones { get; set; } = new();
    public long? ExistingRateCardId { get; set; }
    public bool CreateNew { get; set; } = true;
}

public class ImportedRateCardZone
{
    public string ZoneCategoryName { get; set; } = string.Empty;
    public string ZoneCode { get; set; } = string.Empty;
    public List<ImportedWeightRate> WeightRates { get; set; } = new();
}

public class ImportedWeightRate
{
    public decimal Weight { get; set; }
    public decimal Rate { get; set; }
}

public class RateCardImportError
{
    public string Sheet { get; set; } = string.Empty;
    public int? RowNumber { get; set; }
    public string? Column { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsCritical { get; set; } = true;
}

public class RateCardImportService
{
    public RateCardImportResult ParseExcel(Stream excelStream)
    {
        var result = new RateCardImportResult();
        
        try
        {
            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            
            if (worksheet == null)
            {
                result.Errors.Add(new RateCardImportError
                {
                    Message = "No worksheets found in the Excel file",
                    IsCritical = true
                });
                return result;
            }

            var range = worksheet.RangeUsed();
            if (range == null)
            {
                result.Errors.Add(new RateCardImportError
                {
                    Message = "Worksheet is empty",
                    IsCritical = true
                });
                return result;
            }

            ParseZoneDefinitions(worksheet, result);
            ParseRateCards(worksheet, result);
        }
        catch (Exception ex)
        {
            result.Errors.Add(new RateCardImportError
            {
                Message = $"Error reading Excel file: {ex.Message}",
                IsCritical = true
            });
        }

        return result;
    }

    private void ParseZoneDefinitions(IXLWorksheet worksheet, RateCardImportResult result)
    {
        int headerRow = FindRowContaining(worksheet, "ZONE CATEGORY", 1, 20);
        if (headerRow == 0)
        {
            result.Errors.Add(new RateCardImportError
            {
                Message = "Zone category definition not found. Expected 'ZONE CATEGORY' header in rows 1-20.",
                IsCritical = false
            });
            return;
        }

        var row2 = GetRowValues(worksheet, headerRow, 50);
        var row3 = GetRowValues(worksheet, headerRow + 1, 50);
        
        if (row2.Count < 2)
        {
            result.Errors.Add(new RateCardImportError
            {
                RowNumber = headerRow,
                Message = "Zone category header row is empty or too short",
                IsCritical = false
            });
            return;
        }

        var zoneCategoryDict = new Dictionary<string, ImportedZoneCategory>();
        var currentCol = 1;

        while (currentCol < row2.Count)
        {
            var categoryName = row2.ElementAtOrDefault(currentCol)?.Trim() ?? "";
            var zoneCode = row3.ElementAtOrDefault(currentCol)?.Trim() ?? "";

            if (string.IsNullOrEmpty(categoryName) || string.IsNullOrEmpty(zoneCode))
            {
                currentCol++;
                continue;
            }

            if (!zoneCategoryDict.TryGetValue(categoryName, out var category))
            {
                category = new ImportedZoneCategory { Name = categoryName };
                zoneCategoryDict[categoryName] = category;
            }

            var existingZone = category.Zones.FirstOrDefault(z => z.ZoneCode == zoneCode);
            if (existingZone == null)
            {
                existingZone = new ImportedZone { ZoneCode = zoneCode };
                category.Zones.Add(existingZone);
            }

            int emptyCount = 0;
            for (int row = headerRow + 2; row <= headerRow + 100; row++)
            {
                var countryVal = GetCellValue(worksheet, row, currentCol + 1);
                if (string.IsNullOrEmpty(countryVal))
                {
                    emptyCount++;
                    if (emptyCount >= 3) break;
                    continue;
                }
                emptyCount = 0;
                if (!existingZone.Countries.Contains(countryVal))
                {
                    existingZone.Countries.Add(countryVal);
                }
            }

            currentCol++;
        }

        result.ZoneCategories.AddRange(zoneCategoryDict.Values);
    }

    private int FindRowContaining(IXLWorksheet worksheet, string text, int startRow, int maxRow)
    {
        for (int row = startRow; row <= maxRow; row++)
        {
            for (int col = 1; col <= 10; col++)
            {
                var val = GetCellValue(worksheet, row, col);
                if (val.Equals(text, StringComparison.OrdinalIgnoreCase))
                    return row;
            }
        }
        return 0;
    }

    private string GetCellValue(IXLWorksheet worksheet, int row, int col)
    {
        var cell = worksheet.Cell(row, col);
        if (cell.IsEmpty()) return "";
        
        if (cell.DataType == XLDataType.Number)
            return cell.GetValue<double>().ToString();
        
        return cell.GetString()?.Trim() ?? "";
    }

    private decimal GetCellDecimal(IXLWorksheet worksheet, int row, int col)
    {
        var cell = worksheet.Cell(row, col);
        if (cell.IsEmpty()) return 0;
        
        if (cell.DataType == XLDataType.Number)
            return (decimal)cell.GetValue<double>();
        
        if (decimal.TryParse(cell.GetString()?.Replace(",", ""), out var result))
            return result;
        
        return 0;
    }

    private void ParseRateCards(IXLWorksheet worksheet, RateCardImportResult result)
    {
        var range = worksheet.RangeUsed();
        if (range == null) return;

        var maxRow = range.RowCount();
        int currentRow = 1;

        while (currentRow <= maxRow)
        {
            var cellA = worksheet.Cell(currentRow, 1).GetString()?.Trim() ?? "";
            
            if (cellA.Equals("Rate Card", StringComparison.OrdinalIgnoreCase))
            {
                var rateCardName = worksheet.Cell(currentRow, 2).GetString()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(rateCardName))
                {
                    var rateCard = ParseRateCardSection(worksheet, currentRow, rateCardName, result);
                    if (rateCard != null)
                    {
                        result.RateCards.Add(rateCard);
                    }
                }
            }
            currentRow++;
        }
    }

    private ImportedRateCard? ParseRateCardSection(IXLWorksheet worksheet, int startRow, string rateCardName, RateCardImportResult result)
    {
        var rateCard = new ImportedRateCard { Name = rateCardName };
        
        var headerRow = startRow + 1;
        var headerValues = GetRowValues(worksheet, headerRow, 100);
        
        if (headerValues.Count < 4)
        {
            result.Errors.Add(new RateCardImportError
            {
                RowNumber = headerRow,
                Message = $"Invalid header row for rate card '{rateCardName}'",
                IsCritical = false
            });
            return null;
        }

        var weights = new List<decimal>();
        for (int colIdx = 4; colIdx <= 100; colIdx++)
        {
            var weight = GetCellDecimal(worksheet, headerRow, colIdx);
            if (weight > 0)
            {
                weights.Add(weight);
            }
            else
            {
                var weightStr = GetCellValue(worksheet, headerRow, colIdx);
                if (string.IsNullOrEmpty(weightStr)) break;
                if (decimal.TryParse(weightStr, out var w) && w > 0)
                    weights.Add(w);
            }
        }

        if (!weights.Any())
        {
            result.Errors.Add(new RateCardImportError
            {
                RowNumber = headerRow,
                Message = $"No weight columns found for rate card '{rateCardName}'",
                IsCritical = false
            });
            return null;
        }

        int dataRow = headerRow + 1;
        var range = worksheet.RangeUsed();
        var maxRow = range?.RowCount() ?? 0;

        while (dataRow <= maxRow)
        {
            var cellA = worksheet.Cell(dataRow, 1).GetString()?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(cellA) || cellA.Equals("Rate Card", StringComparison.OrdinalIgnoreCase) || 
                cellA.StartsWith("Notes", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            var zoneCode = worksheet.Cell(dataRow, 2).GetString()?.Trim() ?? "";
            var rateLabel = worksheet.Cell(dataRow, 3).GetString()?.Trim() ?? "";

            if (!string.IsNullOrEmpty(zoneCode) && rateLabel.Equals("Rate", StringComparison.OrdinalIgnoreCase))
            {
                var zone = new ImportedRateCardZone
                {
                    ZoneCategoryName = cellA,
                    ZoneCode = zoneCode
                };

                for (int i = 0; i < weights.Count; i++)
                {
                    var rate = GetCellDecimal(worksheet, dataRow, 4 + i);
                    if (rate > 0)
                    {
                        zone.WeightRates.Add(new ImportedWeightRate
                        {
                            Weight = weights[i],
                            Rate = Math.Round(rate, 2)
                        });
                    }
                }

                if (zone.WeightRates.Any())
                {
                    rateCard.Zones.Add(zone);
                }
            }

            dataRow++;
        }

        return rateCard.Zones.Any() ? rateCard : null;
    }

    private List<string> GetRowValues(IXLWorksheet worksheet, int row, int maxCols)
    {
        var values = new List<string>();
        int consecutiveEmpty = 0;
        for (int col = 1; col <= maxCols; col++)
        {
            var val = GetCellValue(worksheet, row, col);
            if (string.IsNullOrEmpty(val))
            {
                consecutiveEmpty++;
                if (consecutiveEmpty >= 5) break;
            }
            else
            {
                consecutiveEmpty = 0;
            }
            values.Add(val);
        }
        while (values.Count > 0 && string.IsNullOrEmpty(values[^1]))
        {
            values.RemoveAt(values.Count - 1);
        }
        return values;
    }

    public async Task<(int Created, int Updated, List<string> Errors)> ImportRateCards(
        RateCardImportResult parseResult,
        long companyId,
        MovementType movementType,
        Net4Courier.Infrastructure.Data.ApplicationDbContext dbContext)
    {
        var created = 0;
        var updated = 0;
        var errors = new List<string>();

        try
        {
            var existingZoneCategories = dbContext.ZoneCategories
                .Where(z => z.IsActive && !z.IsDeleted)
                .ToList();

            var zoneCategoryMap = new Dictionary<string, long>();

            foreach (var importedCategory in parseResult.ZoneCategories)
            {
                var existing = existingZoneCategories
                    .FirstOrDefault(z => z.Name.Equals(importedCategory.Name, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    zoneCategoryMap[importedCategory.Name.ToUpper()] = existing.Id;
                }
                else if (importedCategory.CreateNew)
                {
                    var newCategory = new ZoneCategory
                    {
                        Code = importedCategory.Name.ToUpper().Substring(0, Math.Min(10, importedCategory.Name.Length)),
                        Name = importedCategory.Name,
                        MovementType = movementType,
                        CreatedAt = DateTime.UtcNow
                    };
                    dbContext.ZoneCategories.Add(newCategory);
                    await dbContext.SaveChangesAsync();
                    zoneCategoryMap[importedCategory.Name.ToUpper()] = newCategory.Id;

                    foreach (var zone in importedCategory.Zones)
                    {
                        var zoneMatrix = new ZoneMatrix
                        {
                            ZoneCategoryId = newCategory.Id,
                            ZoneCode = zone.ZoneCode,
                            ZoneName = $"Zone {zone.ZoneCode}",
                            CompanyId = companyId,
                            CreatedAt = DateTime.UtcNow
                        };
                        dbContext.ZoneMatrices.Add(zoneMatrix);
                    }
                    await dbContext.SaveChangesAsync();
                    created++;
                }
            }

            foreach (var importedRateCard in parseResult.RateCards)
            {
                if (!importedRateCard.CreateNew && importedRateCard.ExistingRateCardId.HasValue)
                {
                    continue;
                }

                foreach (var zoneData in importedRateCard.Zones)
                {
                    var categoryKey = zoneData.ZoneCategoryName.ToUpper();
                    if (!zoneCategoryMap.TryGetValue(categoryKey, out var zoneCategoryId))
                    {
                        errors.Add($"Zone category '{zoneData.ZoneCategoryName}' not found");
                        continue;
                    }

                    var zoneMatrix = dbContext.ZoneMatrices
                        .FirstOrDefault(z => z.ZoneCategoryId == zoneCategoryId && 
                                            z.ZoneCode == zoneData.ZoneCode && 
                                            !z.IsDeleted);

                    if (zoneMatrix == null)
                    {
                        zoneMatrix = new ZoneMatrix
                        {
                            ZoneCategoryId = zoneCategoryId,
                            ZoneCode = zoneData.ZoneCode,
                            ZoneName = $"Zone {zoneData.ZoneCode}",
                            CompanyId = companyId,
                            CreatedAt = DateTime.UtcNow
                        };
                        dbContext.ZoneMatrices.Add(zoneMatrix);
                        await dbContext.SaveChangesAsync();
                    }

                    var rateCardName = $"{importedRateCard.Name} - {zoneData.ZoneCategoryName}";
                    var existingRateCard = dbContext.RateCards
                        .FirstOrDefault(r => r.RateCardName == rateCardName && !r.IsDeleted);

                    RateCard rateCard;
                    if (existingRateCard != null)
                    {
                        rateCard = existingRateCard;
                        updated++;
                    }
                    else
                    {
                        rateCard = new RateCard
                        {
                            RateCardName = rateCardName,
                            MovementTypeId = movementType,
                            ZoneCategoryId = zoneCategoryId,
                            ValidFrom = DateTime.UtcNow.Date,
                            Status = RateCardStatus.Draft,
                            CompanyId = companyId,
                            CreatedAt = DateTime.UtcNow
                        };
                        dbContext.RateCards.Add(rateCard);
                        await dbContext.SaveChangesAsync();
                        created++;
                    }

                    var existingZone = dbContext.RateCardZones
                        .FirstOrDefault(z => z.RateCardId == rateCard.Id && z.ZoneMatrixId == zoneMatrix.Id);

                    RateCardZone rateCardZone;
                    if (existingZone != null)
                    {
                        rateCardZone = existingZone;
                    }
                    else
                    {
                        var baseRate = zoneData.WeightRates.FirstOrDefault()?.Rate ?? 0;
                        rateCardZone = new RateCardZone
                        {
                            RateCardId = rateCard.Id,
                            ZoneMatrixId = zoneMatrix.Id,
                            BaseWeight = 0.5m,
                            SalesBaseRate = baseRate,
                            CostBaseRate = baseRate * 0.8m,
                            SalesPerKg = 0,
                            CostPerKg = 0,
                            CreatedAt = DateTime.UtcNow
                        };
                        dbContext.RateCardZones.Add(rateCardZone);
                        await dbContext.SaveChangesAsync();
                    }

                    var existingSlabs = dbContext.RateCardSlabRules
                        .Where(s => s.RateCardZoneId == rateCardZone.Id)
                        .ToList();
                    dbContext.RateCardSlabRules.RemoveRange(existingSlabs);

                    var sortedRates = zoneData.WeightRates.OrderBy(r => r.Weight).ToList();
                    for (int i = 0; i < sortedRates.Count; i++)
                    {
                        var currentRate = sortedRates[i];
                        var fromWeight = i == 0 ? 0 : sortedRates[i - 1].Weight;
                        var toWeight = currentRate.Weight;
                        var incrementWeight = i == 0 ? 0.5m : (toWeight - fromWeight);
                        var incrementRate = i == 0 ? currentRate.Rate : (currentRate.Rate - sortedRates[i - 1].Rate);

                        var slab = new RateCardSlabRule
                        {
                            RateCardZoneId = rateCardZone.Id,
                            FromWeight = fromWeight,
                            ToWeight = toWeight,
                            IncrementWeight = incrementWeight > 0 ? incrementWeight : 0.5m,
                            IncrementRate = Math.Max(0, incrementRate),
                            CalculationMode = SlabCalculationMode.PerStep,
                            SortOrder = i + 1,
                            CreatedAt = DateTime.UtcNow
                        };
                        dbContext.RateCardSlabRules.Add(slab);
                    }

                    await dbContext.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Import error: {ex.Message}");
        }

        return (created, updated, errors);
    }
}
