using ClosedXML.Excel;
using Net4Courier.Kernel.Enums;
using Net4Courier.Operations.Entities;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Web.Services;

public class ShipmentExcelService
{
    public byte[] GenerateTemplate()
    {
        using var workbook = new XLWorkbook();
        
        var sheet = workbook.Worksheets.Add("Shipments");
        CreateShipmentsSheet(sheet, workbook);
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateTemplateWithMasterData(
        IEnumerable<City> cities,
        IEnumerable<Country> countries,
        IEnumerable<Net4Courier.Masters.Entities.ServiceType>? serviceTypes = null)
    {
        using var workbook = new XLWorkbook();
        
        var sheet = workbook.Worksheets.Add("Shipments");
        CreateShipmentsSheet(sheet, workbook);
        
        CreatePaymentModesSheet(workbook);
        CreateDocumentTypesSheet(workbook);
        CreateCountriesSheet(workbook, countries);
        CreateCitiesSheet(workbook, cities);
        if (serviceTypes != null)
            CreateServiceTypesSheet(workbook, serviceTypes);
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void CreatePaymentModesSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add("Payment Modes");
        
        sheet.Cell(1, 1).Value = "Payment Mode Reference";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 2).Merge();
        
        sheet.Cell(3, 1).Value = "Code";
        sheet.Cell(3, 2).Value = "Description";
        sheet.Cell(3, 1).Style.Font.Bold = true;
        sheet.Cell(3, 2).Style.Font.Bold = true;
        sheet.Cell(3, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        sheet.Cell(3, 2).Style.Fill.BackgroundColor = XLColor.LightBlue;
        
        var paymentModes = new[]
        {
            ("Prepaid", "Payment collected before shipping"),
            ("COD", "Cash on Delivery - collect payment at delivery"),
            ("Account", "Bill to customer account"),
            ("PickupCash", "Cash collected at pickup"),
            ("CAD", "Cash Against Documents")
        };
        
        int row = 4;
        foreach (var (code, desc) in paymentModes)
        {
            sheet.Cell(row, 1).Value = code;
            sheet.Cell(row, 2).Value = desc;
            row++;
        }
        
        sheet.Column(1).Width = 15;
        sheet.Column(2).Width = 40;
    }

    private void CreateDocumentTypesSheet(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add("Document Types");
        
        sheet.Cell(1, 1).Value = "Document Type Reference";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 2).Merge();
        
        sheet.Cell(3, 1).Value = "Code";
        sheet.Cell(3, 2).Value = "Description";
        sheet.Cell(3, 1).Style.Font.Bold = true;
        sheet.Cell(3, 2).Style.Font.Bold = true;
        sheet.Cell(3, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        sheet.Cell(3, 2).Style.Fill.BackgroundColor = XLColor.LightBlue;
        
        var documentTypes = new[]
        {
            ("Document", "Documents, letters, paperwork"),
            ("NonDocument", "Parcels, packages, goods")
        };
        
        int row = 4;
        foreach (var (code, desc) in documentTypes)
        {
            sheet.Cell(row, 1).Value = code;
            sheet.Cell(row, 2).Value = desc;
            row++;
        }
        
        sheet.Column(1).Width = 15;
        sheet.Column(2).Width = 40;
    }

    private void CreateCountriesSheet(XLWorkbook workbook, IEnumerable<Country> countries)
    {
        var sheet = workbook.Worksheets.Add("Countries");
        
        sheet.Cell(1, 1).Value = "Country Reference";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 2).Merge();
        
        sheet.Cell(3, 1).Value = "Code";
        sheet.Cell(3, 2).Value = "Country Name";
        sheet.Cell(3, 1).Style.Font.Bold = true;
        sheet.Cell(3, 2).Style.Font.Bold = true;
        sheet.Cell(3, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        sheet.Cell(3, 2).Style.Fill.BackgroundColor = XLColor.LightBlue;
        
        int row = 4;
        foreach (var country in countries.OrderBy(c => c.Name))
        {
            sheet.Cell(row, 1).Value = country.Code;
            sheet.Cell(row, 2).Value = country.Name;
            row++;
        }
        
        sheet.Column(1).Width = 10;
        sheet.Column(2).Width = 35;
    }

    private void CreateCitiesSheet(XLWorkbook workbook, IEnumerable<City> cities)
    {
        var sheet = workbook.Worksheets.Add("Cities");
        
        sheet.Cell(1, 1).Value = "City Reference";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 3).Merge();
        
        sheet.Cell(3, 1).Value = "Code";
        sheet.Cell(3, 2).Value = "City Name";
        sheet.Cell(3, 3).Value = "Country";
        sheet.Cell(3, 1).Style.Font.Bold = true;
        sheet.Cell(3, 2).Style.Font.Bold = true;
        sheet.Cell(3, 3).Style.Font.Bold = true;
        sheet.Cell(3, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        sheet.Cell(3, 2).Style.Fill.BackgroundColor = XLColor.LightBlue;
        sheet.Cell(3, 3).Style.Fill.BackgroundColor = XLColor.LightBlue;
        
        int row = 4;
        foreach (var city in cities.OrderBy(c => c.Country?.Name).ThenBy(c => c.Name))
        {
            sheet.Cell(row, 1).Value = city.Code;
            sheet.Cell(row, 2).Value = city.Name;
            sheet.Cell(row, 3).Value = city.Country?.Name ?? "";
            row++;
        }
        
        sheet.Column(1).Width = 10;
        sheet.Column(2).Width = 25;
        sheet.Column(3).Width = 25;
    }

    private void CreateServiceTypesSheet(XLWorkbook workbook, IEnumerable<Net4Courier.Masters.Entities.ServiceType> serviceTypes)
    {
        var sheet = workbook.Worksheets.Add("Service Types");
        
        sheet.Cell(1, 1).Value = "Service Type Reference";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 4).Merge();
        
        sheet.Cell(3, 1).Value = "Code";
        sheet.Cell(3, 2).Value = "Name";
        sheet.Cell(3, 3).Value = "Description";
        sheet.Cell(3, 4).Value = "Transit Days";
        sheet.Cell(3, 1).Style.Font.Bold = true;
        sheet.Cell(3, 2).Style.Font.Bold = true;
        sheet.Cell(3, 3).Style.Font.Bold = true;
        sheet.Cell(3, 4).Style.Font.Bold = true;
        sheet.Cell(3, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        sheet.Cell(3, 2).Style.Fill.BackgroundColor = XLColor.LightBlue;
        sheet.Cell(3, 3).Style.Fill.BackgroundColor = XLColor.LightBlue;
        sheet.Cell(3, 4).Style.Fill.BackgroundColor = XLColor.LightBlue;
        
        int row = 4;
        foreach (var serviceType in serviceTypes.OrderBy(s => s.SortOrder).ThenBy(s => s.Name))
        {
            sheet.Cell(row, 1).Value = serviceType.Code;
            sheet.Cell(row, 2).Value = serviceType.Name;
            sheet.Cell(row, 3).Value = serviceType.Description ?? "";
            sheet.Cell(row, 4).Value = serviceType.TransitDays?.ToString() ?? "";
            row++;
        }
        
        sheet.Column(1).Width = 12;
        sheet.Column(2).Width = 25;
        sheet.Column(3).Width = 40;
        sheet.Column(4).Width = 12;
    }

    private void CreateShipmentsSheet(IXLWorksheet sheet, XLWorkbook workbook)
    {
        sheet.Cell(1, 1).Value = "Bulk Shipment Upload Template";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 5).Merge();

        sheet.Cell(2, 1).Value = "Fields marked with * are required";
        sheet.Cell(2, 1).Style.Font.Italic = true;
        sheet.Cell(2, 1).Style.Font.FontColor = XLColor.Red;

        int col = 1;
        var headers = new[]
        {
            ("AWB No (Leave empty for auto-generate)", false),
            ("Transaction Date (DD/MM/YYYY) *", true),
            ("Payment Mode *", true),
            ("Document Type *", true),
            ("Pieces *", true),
            ("Weight (kg) *", true),
            ("Shipper Name *", true),
            ("Shipper Contact", false),
            ("Shipper Phone", false),
            ("Shipper Mobile", false),
            ("Shipper Address 1", false),
            ("Shipper Address 2", false),
            ("Shipper City", false),
            ("Shipper State", false),
            ("Shipper Country", false),
            ("Shipper Postal Code", false),
            ("Consignee Name *", true),
            ("Consignee Contact", false),
            ("Consignee Phone", false),
            ("Consignee Mobile", false),
            ("Consignee Address 1", false),
            ("Consignee Address 2", false),
            ("Consignee City *", true),
            ("Consignee State", false),
            ("Consignee Country *", true),
            ("Consignee Postal Code", false),
            ("Length (cm)", false),
            ("Width (cm)", false),
            ("Height (cm)", false),
            ("Cargo Description *", true),
            ("Special Instructions *", true),
            ("COD Amount *", true),
            ("Reference No *", true)
        };

        foreach (var (header, required) in headers)
        {
            var cell = sheet.Cell(3, col);
            cell.Value = header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = required ? XLColor.LightYellow : XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
        }

        sheet.Row(3).Height = 30;
        sheet.Row(3).Style.Alignment.WrapText = true;
        sheet.Row(3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        var instructions = workbook.Worksheets.Add("Instructions");
        instructions.Cell(1, 1).Value = "Instructions for Bulk Shipment Upload";
        instructions.Cell(1, 1).Style.Font.Bold = true;
        instructions.Cell(1, 1).Style.Font.FontSize = 16;

        int row = 3;
        var instructionList = new[]
        {
            "1. Fill in the shipment data in the 'Shipments' sheet",
            "2. AWB No: Leave empty or enter '0' for auto-generation, or provide your own unique AWB",
            "3. Transaction Date format: DD/MM/YYYY (e.g., 22/01/2026)",
            "4. Payment Mode values: Prepaid, COD, Account, PickupCash, CAD (see Payment Modes tab)",
            "5. Document Type values: Document, NonDocument (see Document Types tab)",
            "6. Weight should be in kilograms",
            "7. Dimensions (Length, Width, Height) should be in centimeters",
            "8. COD Amount is required - use 0 if not applicable, otherwise shipment will be marked as COD",
            "9. Shipper and Consignee details are required for tracking",
            "10. Movement Type (Domestic/International) is auto-calculated based on countries",
            "11. See Countries and Cities tabs for valid country/city codes",
            "12. Cargo Description, Special Instructions, COD Amount, and Reference No are mandatory fields"
        };

        foreach (var instruction in instructionList)
        {
            instructions.Cell(row++, 1).Value = instruction;
        }

        instructions.Column(1).Width = 80;

        for (int i = 1; i <= headers.Length; i++)
        {
            sheet.Column(i).Width = 18;
        }
    }

    public ShipmentExcelParseResult ParseExcel(Stream stream)
    {
        var result = new ShipmentExcelParseResult();
        
        try
        {
            using var workbook = new XLWorkbook(stream);
            
            var sheet = workbook.Worksheet("Shipments");
            if (sheet == null)
            {
                result.Errors.Add(new ShipmentValidationError 
                { 
                    RowNumber = 0,
                    Field = "Sheet", 
                    Message = "Shipments sheet not found in the Excel file" 
                });
                return result;
            }
            
            result.Shipments = ParseShipments(sheet, result.Errors);
        }
        catch (Exception ex)
        {
            result.Errors.Add(new ShipmentValidationError 
            { 
                RowNumber = 0,
                Field = "File", 
                Message = $"Error reading Excel file: {ex.Message}" 
            });
        }
        
        return result;
    }

    private List<ShipmentUploadDto> ParseShipments(IXLWorksheet sheet, List<ShipmentValidationError> errors)
    {
        var shipments = new List<ShipmentUploadDto>();
        
        int row = 4;
        while (true)
        {
            var awbNo = sheet.Cell(row, 1).GetString()?.Trim() ?? "";
            var shipperName = sheet.Cell(row, 7).GetString()?.Trim();
            
            if (string.IsNullOrWhiteSpace(awbNo) && string.IsNullOrWhiteSpace(shipperName))
                break;
            
            if (awbNo == "0") awbNo = "";
            
            var shipment = new ShipmentUploadDto
            {
                RowNumber = row,
                AWBNo = awbNo,
                TransactionDateStr = sheet.Cell(row, 2).GetString()?.Trim() ?? "",
                PaymentModeStr = sheet.Cell(row, 3).GetString()?.Trim() ?? "",
                DocumentTypeStr = sheet.Cell(row, 4).GetString()?.Trim() ?? "",
                Pieces = ParseInt(sheet.Cell(row, 5).GetString()),
                Weight = ParseDecimal(sheet.Cell(row, 6).GetString()),
                ShipperName = sheet.Cell(row, 7).GetString()?.Trim() ?? "",
                ShipperContact = sheet.Cell(row, 8).GetString()?.Trim(),
                ShipperPhone = sheet.Cell(row, 9).GetString()?.Trim(),
                ShipperMobile = sheet.Cell(row, 10).GetString()?.Trim(),
                ShipperAddress1 = sheet.Cell(row, 11).GetString()?.Trim(),
                ShipperAddress2 = sheet.Cell(row, 12).GetString()?.Trim(),
                ShipperCity = sheet.Cell(row, 13).GetString()?.Trim(),
                ShipperState = sheet.Cell(row, 14).GetString()?.Trim(),
                ShipperCountry = sheet.Cell(row, 15).GetString()?.Trim(),
                ShipperPostalCode = sheet.Cell(row, 16).GetString()?.Trim(),
                ConsigneeName = sheet.Cell(row, 17).GetString()?.Trim() ?? "",
                ConsigneeContact = sheet.Cell(row, 18).GetString()?.Trim(),
                ConsigneePhone = sheet.Cell(row, 19).GetString()?.Trim(),
                ConsigneeMobile = sheet.Cell(row, 20).GetString()?.Trim(),
                ConsigneeAddress1 = sheet.Cell(row, 21).GetString()?.Trim(),
                ConsigneeAddress2 = sheet.Cell(row, 22).GetString()?.Trim(),
                ConsigneeCity = sheet.Cell(row, 23).GetString()?.Trim() ?? "",
                ConsigneeState = sheet.Cell(row, 24).GetString()?.Trim(),
                ConsigneeCountry = sheet.Cell(row, 25).GetString()?.Trim() ?? "",
                ConsigneePostalCode = sheet.Cell(row, 26).GetString()?.Trim(),
                Length = ParseDecimal(sheet.Cell(row, 27).GetString()),
                Width = ParseDecimal(sheet.Cell(row, 28).GetString()),
                Height = ParseDecimal(sheet.Cell(row, 29).GetString()),
                CargoDescription = sheet.Cell(row, 30).GetString()?.Trim(),
                SpecialInstructions = sheet.Cell(row, 31).GetString()?.Trim(),
                CODAmount = ParseDecimal(sheet.Cell(row, 32).GetString()),
                ReferenceNo = sheet.Cell(row, 33).GetString()?.Trim()
            };

            ValidateShipment(shipment, errors);
            shipments.Add(shipment);
            row++;
        }
        
        return shipments;
    }

    private void ValidateShipment(ShipmentUploadDto dto, List<ShipmentValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(dto.TransactionDateStr))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Transaction Date", Message = "Transaction Date is required" });
        }
        else if (!TryParseDate(dto.TransactionDateStr, out _))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Transaction Date", Message = "Invalid date format. Use DD/MM/YYYY" });
        }

        if (string.IsNullOrWhiteSpace(dto.PaymentModeStr))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Payment Mode", Message = "Payment Mode is required" });
        }
        else if (!Enum.TryParse<PaymentMode>(dto.PaymentModeStr, true, out _))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Payment Mode", Message = "Invalid Payment Mode. Use: Prepaid, COD, Account, PickupCash, CAD" });
        }

        if (string.IsNullOrWhiteSpace(dto.DocumentTypeStr))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Document Type", Message = "Document Type is required" });
        }
        else if (!Enum.TryParse<DocumentType>(dto.DocumentTypeStr, true, out _))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Document Type", Message = "Invalid Document Type. Use: Document, NonDocument" });
        }

        if (!dto.Pieces.HasValue || dto.Pieces <= 0)
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Pieces", Message = "Pieces must be greater than 0" });
        }

        if (!dto.Weight.HasValue || dto.Weight <= 0)
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Weight", Message = "Weight must be greater than 0" });
        }

        if (string.IsNullOrWhiteSpace(dto.ShipperName))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Shipper Name", Message = "Shipper Name is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.ConsigneeName))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Consignee Name", Message = "Consignee Name is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.ConsigneeCity))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Consignee City", Message = "Consignee City is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.ConsigneeCountry))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Consignee Country", Message = "Consignee Country is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.CargoDescription))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Cargo Description", Message = "Cargo Description is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.SpecialInstructions))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Special Instructions", Message = "Special Instructions is required" });
        }

        if (!dto.CODAmount.HasValue)
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "COD Amount", Message = "COD Amount is required (use 0 if not applicable)" });
        }

        if (string.IsNullOrWhiteSpace(dto.ReferenceNo))
        {
            errors.Add(new ShipmentValidationError { RowNumber = dto.RowNumber, Field = "Reference No", Message = "Reference No is required" });
        }
    }

    private int? ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return int.TryParse(value.Trim(), out var result) ? result : null;
    }

    private decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return decimal.TryParse(value.Trim(), out var result) ? result : null;
    }

    private bool TryParseDate(string value, out DateTime date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(value)) return false;
        
        var formats = new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "yyyy-MM-dd" };
        return DateTime.TryParseExact(value.Trim(), formats, 
            System.Globalization.CultureInfo.InvariantCulture, 
            System.Globalization.DateTimeStyles.None, out date);
    }

    public List<InscanMaster> CreateShipments(List<ShipmentUploadDto> dtos, long? branchId, long? companyId, long? financialYearId, string? defaultOriginCountry = "UAE")
    {
        return dtos.Select(dto => CreateShipment(dto, branchId, companyId, financialYearId, defaultOriginCountry)).ToList();
    }

    public List<InscanMaster> CreateShipmentsWithAutoAWB(
        List<ShipmentUploadDto> dtos, 
        Branch branch, 
        List<BranchAWBConfig> awbConfigs,
        long? companyId, 
        long? financialYearId, 
        string? defaultOriginCountry = "UAE")
    {
        var shipments = new List<InscanMaster>();
        
        var configTrackers = new Dictionary<MovementType, (string Prefix, long CurrentNumber, long Increment, BranchAWBConfig? Config)>();
        
        foreach (MovementType mt in Enum.GetValues<MovementType>())
        {
            var config = awbConfigs.FirstOrDefault(c => c.MovementType == mt && !c.IsDeleted);
            if (config != null)
            {
                var startNum = config.LastUsedNumber > 0 ? config.LastUsedNumber : (config.StartingNumber > 0 ? config.StartingNumber - 1 : 0);
                var prefix = !string.IsNullOrWhiteSpace(config.AWBPrefix) ? config.AWBPrefix : "AWB";
                configTrackers[mt] = (prefix, startNum, config.IncrementBy > 0 ? config.IncrementBy : 1, config);
            }
            else
            {
                var startNum = branch.AWBLastUsedNumber > 0 ? branch.AWBLastUsedNumber : (branch.AWBStartingNumber > 0 ? branch.AWBStartingNumber - 1 : 0);
                var prefix = !string.IsNullOrWhiteSpace(branch.AWBPrefix) ? branch.AWBPrefix : "AWB";
                configTrackers[mt] = (prefix, startNum, branch.AWBIncrement > 0 ? branch.AWBIncrement : 1, null);
            }
        }
        
        foreach (var dto in dtos)
        {
            var shipment = CreateShipment(dto, branch.Id, companyId, financialYearId, defaultOriginCountry);
            
            if (string.IsNullOrWhiteSpace(dto.AWBNo))
            {
                var movementType = shipment.MovementTypeId;
                var tracker = configTrackers[movementType];
                var newNumber = tracker.CurrentNumber + tracker.Increment;
                
                shipment.AWBNo = $"{tracker.Prefix}{newNumber:D7}";
                shipment.IsAutoGenerated = true;
                dto.AWBNo = shipment.AWBNo;
                
                configTrackers[movementType] = (tracker.Prefix, newNumber, tracker.Increment, tracker.Config);
            }
            
            shipments.Add(shipment);
        }
        
        foreach (var kvp in configTrackers)
        {
            if (kvp.Value.Config != null)
            {
                kvp.Value.Config.LastUsedNumber = kvp.Value.CurrentNumber;
                kvp.Value.Config.ModifiedAt = DateTime.UtcNow;
            }
        }
        
        var usedBranchFallback = configTrackers.Values.Any(v => v.Config == null && v.CurrentNumber > (branch.AWBLastUsedNumber > 0 ? branch.AWBLastUsedNumber : branch.AWBStartingNumber - 1));
        if (usedBranchFallback)
        {
            var maxBranchNumber = configTrackers.Values.Where(v => v.Config == null).Select(v => v.CurrentNumber).DefaultIfEmpty(0).Max();
            if (maxBranchNumber > branch.AWBLastUsedNumber)
            {
                branch.AWBLastUsedNumber = maxBranchNumber;
            }
        }
        
        return shipments;
    }

    public int CountAutoGenerateAWBs(List<ShipmentUploadDto> dtos)
    {
        return dtos.Count(d => string.IsNullOrWhiteSpace(d.AWBNo));
    }

    private InscanMaster CreateShipment(ShipmentUploadDto dto, long? branchId, long? companyId, long? financialYearId, string? defaultOriginCountry)
    {
        TryParseDate(dto.TransactionDateStr, out var transactionDate);
        Enum.TryParse<PaymentMode>(dto.PaymentModeStr, true, out var paymentMode);
        Enum.TryParse<DocumentType>(dto.DocumentTypeStr, true, out var documentType);

        var shipperCountry = string.IsNullOrWhiteSpace(dto.ShipperCountry) ? defaultOriginCountry : dto.ShipperCountry;
        var movementType = DetermineMovementType(shipperCountry, dto.ConsigneeCountry);

        var shipment = new InscanMaster
        {
            AWBNo = dto.AWBNo,
            TransactionDate = transactionDate == default ? DateTime.UtcNow : DateTime.SpecifyKind(transactionDate, DateTimeKind.Utc),
            FinancialYearId = financialYearId,
            CompanyId = companyId,
            BranchId = branchId,
            PaymentModeId = paymentMode,
            DocumentTypeId = documentType,
            MovementTypeId = movementType,
            Pieces = dto.Pieces ?? 1,
            Weight = dto.Weight ?? 0,
            StatedWeight = dto.Weight ?? 0,
            ChargeableWeight = dto.Weight ?? 0,
            Consignor = dto.ShipperName,
            ConsignorContact = dto.ShipperContact,
            ConsignorPhone = dto.ShipperPhone,
            ConsignorMobile = dto.ShipperMobile,
            ConsignorAddress1 = dto.ShipperAddress1,
            ConsignorAddress2 = dto.ShipperAddress2,
            ConsignorCity = dto.ShipperCity,
            ConsignorState = dto.ShipperState,
            ConsignorCountry = shipperCountry,
            ConsignorPostalCode = dto.ShipperPostalCode,
            Consignee = dto.ConsigneeName,
            ConsigneeContact = dto.ConsigneeContact,
            ConsigneePhone = dto.ConsigneePhone,
            ConsigneeMobile = dto.ConsigneeMobile,
            ConsigneeAddress1 = dto.ConsigneeAddress1,
            ConsigneeAddress2 = dto.ConsigneeAddress2,
            ConsigneeCity = dto.ConsigneeCity,
            ConsigneeState = dto.ConsigneeState,
            ConsigneeCountry = dto.ConsigneeCountry,
            ConsigneePostalCode = dto.ConsigneePostalCode,
            Length = dto.Length,
            Width = dto.Width,
            Height = dto.Height,
            CargoDescription = dto.CargoDescription,
            SpecialInstructions = dto.SpecialInstructions,
            IsCOD = dto.CODAmount.HasValue && dto.CODAmount > 0,
            CODAmount = dto.CODAmount,
            ReferenceNo = dto.ReferenceNo,
            CourierStatusId = CourierStatus.Pending,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Length.HasValue && dto.Width.HasValue && dto.Height.HasValue)
        {
            shipment.VolumetricWeight = (dto.Length.Value * dto.Width.Value * dto.Height.Value) / 5000m;
            shipment.CBM = (dto.Length.Value * dto.Width.Value * dto.Height.Value) / 1000000m;
            shipment.ChargeableWeight = Math.Max(shipment.Weight ?? 0, shipment.VolumetricWeight ?? 0);
        }

        return shipment;
    }

    private MovementType DetermineMovementType(string? originCountry, string? destCountry)
    {
        if (string.IsNullOrWhiteSpace(originCountry) || string.IsNullOrWhiteSpace(destCountry))
            return MovementType.Domestic;

        var origin = originCountry.Trim().ToUpperInvariant();
        var dest = destCountry.Trim().ToUpperInvariant();

        var uaeVariants = new[] { "UAE", "UNITED ARAB EMIRATES", "U.A.E.", "EMIRATES" };
        
        var isOriginUAE = uaeVariants.Any(v => origin.Contains(v));
        var isDestUAE = uaeVariants.Any(v => dest.Contains(v));

        if (isOriginUAE && isDestUAE)
            return MovementType.Domestic;
        
        return MovementType.InternationalExport;
    }
}

public class ShipmentExcelParseResult
{
    public List<ShipmentUploadDto> Shipments { get; set; } = new();
    public List<ShipmentValidationError> Errors { get; set; } = new();
    public bool HasErrors => Errors.Count > 0;
}

public class ShipmentValidationError
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ShipmentUploadDto
{
    public int RowNumber { get; set; }
    public string AWBNo { get; set; } = string.Empty;
    public string TransactionDateStr { get; set; } = string.Empty;
    public string PaymentModeStr { get; set; } = string.Empty;
    public string DocumentTypeStr { get; set; } = string.Empty;
    public int? Pieces { get; set; }
    public decimal? Weight { get; set; }
    public string ShipperName { get; set; } = string.Empty;
    public string? ShipperContact { get; set; }
    public string? ShipperPhone { get; set; }
    public string? ShipperMobile { get; set; }
    public string? ShipperAddress1 { get; set; }
    public string? ShipperAddress2 { get; set; }
    public string? ShipperCity { get; set; }
    public string? ShipperState { get; set; }
    public string? ShipperCountry { get; set; }
    public string? ShipperPostalCode { get; set; }
    public string ConsigneeName { get; set; } = string.Empty;
    public string? ConsigneeContact { get; set; }
    public string? ConsigneePhone { get; set; }
    public string? ConsigneeMobile { get; set; }
    public string? ConsigneeAddress1 { get; set; }
    public string? ConsigneeAddress2 { get; set; }
    public string ConsigneeCity { get; set; } = string.Empty;
    public string? ConsigneeState { get; set; }
    public string ConsigneeCountry { get; set; } = string.Empty;
    public string? ConsigneePostalCode { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? CargoDescription { get; set; }
    public string? SpecialInstructions { get; set; }
    public decimal? CODAmount { get; set; }
    public string? ReferenceNo { get; set; }
}
