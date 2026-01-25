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

    public byte[] GenerateTemplate()
    {
        using var workbook = new XLWorkbook();
        
        var dataSheet = workbook.Worksheets.Add("Rate Card Data");
        
        dataSheet.Cell(1, 1).Value = "ZONE CATEGORY";
        dataSheet.Cell(1, 2).Value = "DHL";
        dataSheet.Cell(1, 3).Value = "DHL";
        dataSheet.Cell(1, 4).Value = "DHL";
        dataSheet.Cell(1, 5).Value = "FEDEX";
        dataSheet.Cell(1, 6).Value = "FEDEX";
        dataSheet.Cell(1, 7).Value = "ARAMEX";
        dataSheet.Cell(1, 8).Value = "ARAMEX";
        
        dataSheet.Cell(2, 1).Value = "ZONE";
        dataSheet.Cell(2, 2).Value = "A";
        dataSheet.Cell(2, 3).Value = "B";
        dataSheet.Cell(2, 4).Value = "C";
        dataSheet.Cell(2, 5).Value = "A";
        dataSheet.Cell(2, 6).Value = "B";
        dataSheet.Cell(2, 7).Value = "A";
        dataSheet.Cell(2, 8).Value = "B";
        
        dataSheet.Cell(3, 1).Value = "COUNTRIES";
        dataSheet.Cell(3, 2).Value = "USA";
        dataSheet.Cell(3, 3).Value = "UK";
        dataSheet.Cell(3, 4).Value = "Australia";
        dataSheet.Cell(3, 5).Value = "USA";
        dataSheet.Cell(3, 6).Value = "Canada";
        dataSheet.Cell(3, 7).Value = "India";
        dataSheet.Cell(3, 8).Value = "China";
        
        dataSheet.Cell(4, 2).Value = "Canada";
        dataSheet.Cell(4, 3).Value = "Germany";
        dataSheet.Cell(4, 4).Value = "New Zealand";
        dataSheet.Cell(4, 5).Value = "Mexico";
        dataSheet.Cell(4, 6).Value = "UK";
        dataSheet.Cell(4, 7).Value = "Pakistan";
        dataSheet.Cell(4, 8).Value = "Japan";
        
        dataSheet.Cell(5, 2).Value = "Mexico";
        dataSheet.Cell(5, 3).Value = "France";
        
        var headerRange = dataSheet.Range("A1:H2");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        
        int rateCardStartRow = 8;
        string[] rateCardNames = { "Economy", "Next Day", "Priority" };
        decimal[] weights = { 0.5m, 1m, 1.5m, 2m, 2.5m, 3m, 4m, 5m, 6m, 7m, 8m, 9m, 10m, 11m };
        
        (string Category, string Zone)[] zones = {
            ("DHL", "A"), ("DHL", "B"), ("DHL", "C"),
            ("FEDEX", "A"), ("FEDEX", "B"),
            ("ARAMEX", "A"), ("ARAMEX", "B")
        };
        
        foreach (var cardName in rateCardNames)
        {
            dataSheet.Cell(rateCardStartRow, 1).Value = "Rate Card";
            dataSheet.Cell(rateCardStartRow, 2).Value = cardName;
            dataSheet.Cell(rateCardStartRow, 1).Style.Font.Bold = true;
            dataSheet.Cell(rateCardStartRow, 2).Style.Font.Bold = true;
            dataSheet.Range(rateCardStartRow, 1, rateCardStartRow, 2).Style.Fill.BackgroundColor = XLColor.LightGreen;
            
            int headerRow = rateCardStartRow + 1;
            dataSheet.Cell(headerRow, 1).Value = "Category";
            dataSheet.Cell(headerRow, 2).Value = "Zone";
            dataSheet.Cell(headerRow, 3).Value = "Rate";
            for (int i = 0; i < weights.Length; i++)
            {
                dataSheet.Cell(headerRow, i + 4).Value = weights[i];
            }
            dataSheet.Range(headerRow, 1, headerRow, weights.Length + 3).Style.Font.Bold = true;
            dataSheet.Range(headerRow, 1, headerRow, weights.Length + 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            
            for (int z = 0; z < zones.Length; z++)
            {
                int row = headerRow + 1 + z;
                dataSheet.Cell(row, 1).Value = zones[z].Category;
                dataSheet.Cell(row, 2).Value = zones[z].Zone;
                dataSheet.Cell(row, 3).Value = "Rate";
                
                var random = new Random(cardName.GetHashCode() + z);
                for (int w = 0; w < weights.Length; w++)
                {
                    decimal baseRate = 10 + (z * 5) + (weights[w] * 2);
                    dataSheet.Cell(row, w + 4).Value = Math.Round(baseRate + (decimal)(random.NextDouble() * 5), 2);
                }
            }
            
            rateCardStartRow += zones.Length + 4;
        }
        
        dataSheet.Columns().AdjustToContents();
        
        var instructionsSheet = workbook.Worksheets.Add("Instructions");
        instructionsSheet.Cell(1, 1).Value = "Rate Card Import Template Instructions";
        instructionsSheet.Cell(1, 1).Style.Font.Bold = true;
        instructionsSheet.Cell(1, 1).Style.Font.FontSize = 14;
        
        instructionsSheet.Cell(3, 1).Value = "ZONE DEFINITIONS (Rows 1-5+):";
        instructionsSheet.Cell(3, 1).Style.Font.Bold = true;
        instructionsSheet.Cell(4, 1).Value = "- Row 1: 'ZONE CATEGORY' in column A, then carrier names (DHL, FEDEX, etc.) in columns B onwards";
        instructionsSheet.Cell(5, 1).Value = "- Row 2: Zone codes (A, B, C, etc.) under each carrier";
        instructionsSheet.Cell(6, 1).Value = "- Rows 3+: Country names under each zone column (one country per row)";
        
        instructionsSheet.Cell(8, 1).Value = "RATE CARDS:";
        instructionsSheet.Cell(8, 1).Style.Font.Bold = true;
        instructionsSheet.Cell(9, 1).Value = "- Start with 'Rate Card' in column A and the rate card name in column B";
        instructionsSheet.Cell(10, 1).Value = "- Header row: Category, Zone, Rate, then weight values (0.5, 1, 1.5, etc.)";
        instructionsSheet.Cell(11, 1).Value = "- Data rows: Carrier name in column A, Zone code in column B, 'Rate' in column C, then rate values";
        
        instructionsSheet.Cell(13, 1).Value = "NOTES:";
        instructionsSheet.Cell(13, 1).Style.Font.Bold = true;
        instructionsSheet.Cell(14, 1).Value = "- Carrier names in data rows must match zone category names exactly";
        instructionsSheet.Cell(15, 1).Value = "- Zone codes in data rows must match zone codes in zone definitions";
        instructionsSheet.Cell(16, 1).Value = "- Multiple rate cards can be defined in the same sheet";
        instructionsSheet.Cell(17, 1).Value = "- Leave blank rows between rate card sections";
        
        instructionsSheet.Column(1).Width = 80;
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
