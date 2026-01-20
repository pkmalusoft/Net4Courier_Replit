using ClosedXML.Excel;
using Net4Courier.Kernel.Enums;
using Net4Courier.Operations.Entities;

namespace Net4Courier.Web.Services;

public class ImportExcelParseResult
{
    public ImportHeaderDto? Header { get; set; }
    public List<ImportShipmentDto> Shipments { get; set; } = new();
    public List<ImportValidationError> Errors { get; set; } = new();
    public bool IsValid => !Errors.Any(e => e.IsCritical);
}

public class ImportHeaderDto
{
    public ImportMode ImportMode { get; set; } = ImportMode.Air;
    public string MasterReferenceNumber { get; set; } = string.Empty;
    public string? OriginCountry { get; set; }
    public string? OriginCity { get; set; }
    public string? OriginPort { get; set; }
    public string? DestinationCountry { get; set; }
    public string? DestinationCity { get; set; }
    public string? DestinationPort { get; set; }
    public DateTime? ETD { get; set; }
    public DateTime? ETA { get; set; }
    public string? CarrierName { get; set; }
    public string? CarrierCode { get; set; }
    public string? FlightNo { get; set; }
    public DateTime? FlightDate { get; set; }
    public string? VesselName { get; set; }
    public string? VoyageNumber { get; set; }
    public string? TruckNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? ManifestNumber { get; set; }
    public string? Remarks { get; set; }
}

public class ImportShipmentDto
{
    public int RowNumber { get; set; }
    public string AWBNo { get; set; } = string.Empty;
    public string ConsigneeName { get; set; } = string.Empty;
    public string? ConsigneeAddress { get; set; }
    public string? ConsigneeCity { get; set; }
    public string? ConsigneeState { get; set; }
    public string ConsigneeCountry { get; set; } = string.Empty;
    public string? ConsigneePostalCode { get; set; }
    public string? ConsigneePhone { get; set; }
    public string? ShipperName { get; set; }
    public string? ShipperCountry { get; set; }
    public int Pieces { get; set; } = 1;
    public decimal Weight { get; set; }
    public string? ContentsDescription { get; set; }
    public string? HSCode { get; set; }
    public decimal? DeclaredValue { get; set; }
    public string? Currency { get; set; }
    public string? PaymentMode { get; set; }
    public string? SpecialInstructions { get; set; }
}

public class ImportValidationError
{
    public string Sheet { get; set; } = string.Empty;
    public int? RowNumber { get; set; }
    public string? Column { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsCritical { get; set; } = true;
}

public class ImportExcelService
{
    public byte[] GenerateTemplate(ImportMode mode)
    {
        using var workbook = new XLWorkbook();
        
        var headerSheet = workbook.Worksheets.Add("Header");
        CreateHeaderSheet(headerSheet, mode);
        
        var shipmentsSheet = workbook.Worksheets.Add("Shipments");
        CreateShipmentsSheet(shipmentsSheet);
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void CreateHeaderSheet(IXLWorksheet sheet, ImportMode mode)
    {
        sheet.Cell(1, 1).Value = "Import Template - Header Information";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 2).Merge();
        
        int row = 3;
        
        sheet.Cell(row, 1).Value = "Field";
        sheet.Cell(row, 2).Value = "Value";
        sheet.Cell(row, 3).Value = "Required";
        sheet.Cell(row, 4).Value = "Description";
        sheet.Range(row, 1, row, 4).Style.Font.Bold = true;
        sheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;
        
        AddHeaderRow(sheet, ref row, "Transport Mode", mode.ToString(), "Yes", "Air, Sea, or Land");
        AddHeaderRow(sheet, ref row, "Master Reference", "", "Yes", mode == ImportMode.Air ? "MAWB Number" : mode == ImportMode.Sea ? "Bill of Lading Number" : "Truck/Vehicle Number");
        AddHeaderRow(sheet, ref row, "Origin Country", "", "Yes", "Country of origin");
        AddHeaderRow(sheet, ref row, "Origin City", "", "No", "City of origin");
        AddHeaderRow(sheet, ref row, "Origin Port/Airport", "", "No", mode == ImportMode.Air ? "Origin Airport Code (e.g., DXB)" : "Origin Port Code");
        AddHeaderRow(sheet, ref row, "Destination Country", "", "Yes", "Destination country");
        AddHeaderRow(sheet, ref row, "Destination City", "", "No", "Destination city");
        AddHeaderRow(sheet, ref row, "Destination Port/Airport", "", "No", mode == ImportMode.Air ? "Destination Airport Code" : "Destination Port Code");
        AddHeaderRow(sheet, ref row, "ETD", "", "No", "Estimated Time of Departure (DD/MM/YYYY)");
        AddHeaderRow(sheet, ref row, "ETA", "", "No", "Estimated Time of Arrival (DD/MM/YYYY)");
        AddHeaderRow(sheet, ref row, "Carrier Name", "", "No", "Carrier/Airline/Shipping Line name");
        AddHeaderRow(sheet, ref row, "Carrier Code", "", "No", "Carrier code (e.g., EK, BA)");
        
        if (mode == ImportMode.Air)
        {
            AddHeaderRow(sheet, ref row, "Flight Number", "", "No", "Flight number (e.g., EK123)");
            AddHeaderRow(sheet, ref row, "Flight Date", "", "No", "Flight date (DD/MM/YYYY)");
        }
        else if (mode == ImportMode.Sea)
        {
            AddHeaderRow(sheet, ref row, "Vessel Name", "", "No", "Name of the vessel");
            AddHeaderRow(sheet, ref row, "Voyage Number", "", "No", "Voyage/Trip number");
        }
        else
        {
            AddHeaderRow(sheet, ref row, "Truck Number", "", "No", "Vehicle registration number");
            AddHeaderRow(sheet, ref row, "Driver Name", "", "No", "Driver's name");
            AddHeaderRow(sheet, ref row, "Driver Phone", "", "No", "Driver's contact number");
        }
        
        AddHeaderRow(sheet, ref row, "Manifest Number", "", "No", "Manifest/Cargo number");
        AddHeaderRow(sheet, ref row, "Remarks", "", "No", "Additional remarks");
        
        sheet.Columns().AdjustToContents();
        sheet.Column(2).Width = 30;
        sheet.Column(4).Width = 40;
    }

    private void AddHeaderRow(IXLWorksheet sheet, ref int row, string field, string value, string required, string description)
    {
        sheet.Cell(row, 1).Value = field;
        sheet.Cell(row, 2).Value = value;
        sheet.Cell(row, 3).Value = required;
        sheet.Cell(row, 4).Value = description;
        if (required == "Yes")
        {
            sheet.Cell(row, 1).Style.Font.Bold = true;
        }
        row++;
    }

    private void CreateShipmentsSheet(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "Shipment Details";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        
        int col = 1;
        var headers = new[]
        {
            ("AWB No *", true),
            ("Consignee Name *", true),
            ("Consignee Address", false),
            ("Consignee City", false),
            ("Consignee State", false),
            ("Consignee Country *", true),
            ("Consignee Postal Code", false),
            ("Consignee Phone", false),
            ("Shipper Name", false),
            ("Shipper Country", false),
            ("Pieces *", true),
            ("Weight (kg) *", true),
            ("Contents Description", false),
            ("HS Code", false),
            ("Declared Value", false),
            ("Currency", false),
            ("Payment Mode", false),
            ("Special Instructions", false)
        };

        foreach (var (header, required) in headers)
        {
            sheet.Cell(3, col).Value = header;
            sheet.Cell(3, col).Style.Font.Bold = true;
            sheet.Cell(3, col).Style.Fill.BackgroundColor = required ? XLColor.LightBlue : XLColor.LightGray;
            col++;
        }
        
        sheet.Range(3, 1, 3, headers.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        sheet.Range(3, 1, 3, headers.Length).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        
        sheet.Columns().AdjustToContents();
    }

    public ImportExcelParseResult ParseExcel(Stream stream)
    {
        var result = new ImportExcelParseResult();
        
        try
        {
            using var workbook = new XLWorkbook(stream);
            
            var headerSheet = workbook.Worksheet("Header");
            if (headerSheet == null)
            {
                result.Errors.Add(new ImportValidationError 
                { 
                    Sheet = "Header", 
                    Message = "Header sheet not found in the Excel file" 
                });
                return result;
            }
            
            result.Header = ParseHeader(headerSheet, result.Errors);
            
            var shipmentsSheet = workbook.Worksheet("Shipments");
            if (shipmentsSheet == null)
            {
                result.Errors.Add(new ImportValidationError 
                { 
                    Sheet = "Shipments", 
                    Message = "Shipments sheet not found in the Excel file" 
                });
                return result;
            }
            
            result.Shipments = ParseShipments(shipmentsSheet, result.Errors);
        }
        catch (Exception ex)
        {
            result.Errors.Add(new ImportValidationError 
            { 
                Sheet = "File", 
                Message = $"Error reading Excel file: {ex.Message}" 
            });
        }
        
        return result;
    }

    private ImportHeaderDto ParseHeader(IXLWorksheet sheet, List<ImportValidationError> errors)
    {
        var header = new ImportHeaderDto();
        var fieldValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        for (int row = 4; row <= 25; row++)
        {
            var field = sheet.Cell(row, 1).GetString()?.Trim();
            var value = sheet.Cell(row, 2).GetString()?.Trim();
            if (!string.IsNullOrEmpty(field))
            {
                fieldValues[field] = value ?? "";
            }
        }
        
        if (fieldValues.TryGetValue("Transport Mode", out var modeStr))
        {
            if (Enum.TryParse<ImportMode>(modeStr, true, out var mode))
                header.ImportMode = mode;
        }
        
        if (fieldValues.TryGetValue("Master Reference", out var masterRef))
        {
            header.MasterReferenceNumber = masterRef;
            if (string.IsNullOrWhiteSpace(masterRef))
            {
                errors.Add(new ImportValidationError 
                { 
                    Sheet = "Header", 
                    Column = "Master Reference", 
                    Message = "Master Reference is required" 
                });
            }
        }
        
        if (fieldValues.TryGetValue("Origin Country", out var originCountry))
        {
            header.OriginCountry = originCountry;
            if (string.IsNullOrWhiteSpace(originCountry))
            {
                errors.Add(new ImportValidationError 
                { 
                    Sheet = "Header", 
                    Column = "Origin Country", 
                    Message = "Origin Country is required" 
                });
            }
        }
        
        if (fieldValues.TryGetValue("Origin City", out var originCity))
            header.OriginCity = originCity;
        if (fieldValues.TryGetValue("Origin Port/Airport", out var originPort))
            header.OriginPort = originPort;
        
        if (fieldValues.TryGetValue("Destination Country", out var destCountry))
        {
            header.DestinationCountry = destCountry;
            if (string.IsNullOrWhiteSpace(destCountry))
            {
                errors.Add(new ImportValidationError 
                { 
                    Sheet = "Header", 
                    Column = "Destination Country", 
                    Message = "Destination Country is required" 
                });
            }
        }
        
        if (fieldValues.TryGetValue("Destination City", out var destCity))
            header.DestinationCity = destCity;
        if (fieldValues.TryGetValue("Destination Port/Airport", out var destPort))
            header.DestinationPort = destPort;
        
        if (fieldValues.TryGetValue("ETD", out var etdStr) && !string.IsNullOrWhiteSpace(etdStr))
        {
            if (DateTime.TryParse(etdStr, out var etd))
                header.ETD = etd;
        }
        if (fieldValues.TryGetValue("ETA", out var etaStr) && !string.IsNullOrWhiteSpace(etaStr))
        {
            if (DateTime.TryParse(etaStr, out var eta))
                header.ETA = eta;
        }
        
        if (fieldValues.TryGetValue("Carrier Name", out var carrierName))
            header.CarrierName = carrierName;
        if (fieldValues.TryGetValue("Carrier Code", out var carrierCode))
            header.CarrierCode = carrierCode;
        
        if (fieldValues.TryGetValue("Flight Number", out var flightNo))
            header.FlightNo = flightNo;
        if (fieldValues.TryGetValue("Flight Date", out var flightDateStr) && !string.IsNullOrWhiteSpace(flightDateStr))
        {
            if (DateTime.TryParse(flightDateStr, out var flightDate))
                header.FlightDate = flightDate;
        }
        
        if (fieldValues.TryGetValue("Vessel Name", out var vesselName))
            header.VesselName = vesselName;
        if (fieldValues.TryGetValue("Voyage Number", out var voyageNo))
            header.VoyageNumber = voyageNo;
        
        if (fieldValues.TryGetValue("Truck Number", out var truckNo))
            header.TruckNumber = truckNo;
        if (fieldValues.TryGetValue("Driver Name", out var driverName))
            header.DriverName = driverName;
        if (fieldValues.TryGetValue("Driver Phone", out var driverPhone))
            header.DriverPhone = driverPhone;
        
        if (fieldValues.TryGetValue("Manifest Number", out var manifestNo))
            header.ManifestNumber = manifestNo;
        if (fieldValues.TryGetValue("Remarks", out var remarks))
            header.Remarks = remarks;
        
        return header;
    }

    private List<ImportShipmentDto> ParseShipments(IXLWorksheet sheet, List<ImportValidationError> errors)
    {
        var shipments = new List<ImportShipmentDto>();
        
        int row = 4;
        while (true)
        {
            var awbNo = sheet.Cell(row, 1).GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(awbNo))
                break;
            
            var shipment = new ImportShipmentDto
            {
                RowNumber = row,
                AWBNo = awbNo,
                ConsigneeName = sheet.Cell(row, 2).GetString()?.Trim() ?? "",
                ConsigneeAddress = sheet.Cell(row, 3).GetString()?.Trim(),
                ConsigneeCity = sheet.Cell(row, 4).GetString()?.Trim(),
                ConsigneeState = sheet.Cell(row, 5).GetString()?.Trim(),
                ConsigneeCountry = sheet.Cell(row, 6).GetString()?.Trim() ?? "",
                ConsigneePostalCode = sheet.Cell(row, 7).GetString()?.Trim(),
                ConsigneePhone = sheet.Cell(row, 8).GetString()?.Trim(),
                ShipperName = sheet.Cell(row, 9).GetString()?.Trim(),
                ShipperCountry = sheet.Cell(row, 10).GetString()?.Trim(),
                ContentsDescription = sheet.Cell(row, 13).GetString()?.Trim(),
                HSCode = sheet.Cell(row, 14).GetString()?.Trim(),
                Currency = sheet.Cell(row, 16).GetString()?.Trim(),
                PaymentMode = sheet.Cell(row, 17).GetString()?.Trim(),
                SpecialInstructions = sheet.Cell(row, 18).GetString()?.Trim()
            };
            
            var piecesVal = sheet.Cell(row, 11).Value;
            if (piecesVal.IsNumber)
                shipment.Pieces = (int)piecesVal.GetNumber();
            else if (int.TryParse(sheet.Cell(row, 11).GetString(), out var pieces))
                shipment.Pieces = pieces;
            else
                shipment.Pieces = 1;
            
            var weightVal = sheet.Cell(row, 12).Value;
            if (weightVal.IsNumber)
                shipment.Weight = (decimal)weightVal.GetNumber();
            else if (decimal.TryParse(sheet.Cell(row, 12).GetString(), out var weight))
                shipment.Weight = weight;
            
            var declaredVal = sheet.Cell(row, 15).Value;
            if (declaredVal.IsNumber)
                shipment.DeclaredValue = (decimal)declaredVal.GetNumber();
            else if (decimal.TryParse(sheet.Cell(row, 15).GetString(), out var declaredValue))
                shipment.DeclaredValue = declaredValue;
            
            if (string.IsNullOrWhiteSpace(shipment.ConsigneeName))
            {
                errors.Add(new ImportValidationError 
                { 
                    Sheet = "Shipments", 
                    RowNumber = row, 
                    Column = "Consignee Name", 
                    Message = "Consignee Name is required" 
                });
            }
            
            if (string.IsNullOrWhiteSpace(shipment.ConsigneeCountry))
            {
                errors.Add(new ImportValidationError 
                { 
                    Sheet = "Shipments", 
                    RowNumber = row, 
                    Column = "Consignee Country", 
                    Message = "Consignee Country is required" 
                });
            }
            
            if (shipment.Weight <= 0)
            {
                errors.Add(new ImportValidationError 
                { 
                    Sheet = "Shipments", 
                    RowNumber = row, 
                    Column = "Weight", 
                    Message = "Weight must be greater than 0" 
                });
            }
            
            if (shipment.Pieces <= 0)
            {
                errors.Add(new ImportValidationError 
                { 
                    Sheet = "Shipments", 
                    RowNumber = row, 
                    Column = "Pieces", 
                    Message = "Pieces must be greater than 0" 
                });
            }
            
            shipments.Add(shipment);
            row++;
            
            if (row > 1000)
            {
                errors.Add(new ImportValidationError 
                { 
                    Sheet = "Shipments", 
                    Message = "Maximum 1000 shipments allowed per import",
                    IsCritical = false
                });
                break;
            }
        }
        
        if (shipments.Count == 0)
        {
            errors.Add(new ImportValidationError 
            { 
                Sheet = "Shipments", 
                Message = "No shipments found in the Excel file" 
            });
        }
        
        var duplicateAwbs = shipments
            .GroupBy(s => s.AWBNo.ToUpperInvariant())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        
        foreach (var dup in duplicateAwbs)
        {
            var dupRows = shipments.Where(s => s.AWBNo.ToUpperInvariant() == dup).Select(s => s.RowNumber);
            errors.Add(new ImportValidationError 
            { 
                Sheet = "Shipments", 
                Column = "AWB No",
                Message = $"Duplicate AWB '{dup}' found in rows {string.Join(", ", dupRows)}" 
            });
        }
        
        return shipments;
    }

    public ImportMaster CreateImportMaster(ImportHeaderDto header, long? branchId, long? companyId, long? financialYearId)
    {
        var refPrefix = header.ImportMode switch
        {
            ImportMode.Air => "IMP-AIR",
            ImportMode.Sea => "IMP-SEA",
            ImportMode.Land => "IMP-LND",
            _ => "IMP"
        };
        
        return new ImportMaster
        {
            ImportRefNo = $"{refPrefix}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            TransactionDate = DateTime.UtcNow,
            FinancialYearId = financialYearId,
            CompanyId = companyId,
            BranchId = branchId,
            ImportMode = header.ImportMode,
            MasterReferenceType = header.ImportMode == ImportMode.Air ? MasterReferenceType.MAWB : 
                                  header.ImportMode == ImportMode.Sea ? MasterReferenceType.BL : MasterReferenceType.TruckWaybill,
            MasterReferenceNumber = header.MasterReferenceNumber,
            OriginCountryName = header.OriginCountry,
            OriginCityName = header.OriginCity,
            OriginPortCode = header.OriginPort,
            DestinationCountryName = header.DestinationCountry,
            DestinationCityName = header.DestinationCity,
            DestinationPortCode = header.DestinationPort,
            ETD = header.ETD,
            ETA = header.ETA,
            CarrierName = header.CarrierName,
            CarrierCode = header.CarrierCode,
            FlightNo = header.FlightNo,
            FlightDate = header.FlightDate,
            VesselName = header.VesselName,
            VoyageNumber = header.VoyageNumber,
            TruckNumber = header.TruckNumber,
            DriverName = header.DriverName,
            DriverPhone = header.DriverPhone,
            ManifestNumber = header.ManifestNumber,
            Remarks = header.Remarks,
            Status = ImportMasterStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
    }

    public List<ImportShipment> CreateImportShipments(List<ImportShipmentDto> shipmentDtos, long importMasterId)
    {
        return shipmentDtos.Select(dto => new ImportShipment
        {
            ImportMasterId = importMasterId,
            AWBNo = dto.AWBNo,
            ConsigneeName = dto.ConsigneeName,
            ConsigneeAddress = dto.ConsigneeAddress,
            ConsigneeCity = dto.ConsigneeCity,
            ConsigneeState = dto.ConsigneeState,
            ConsigneeCountry = dto.ConsigneeCountry,
            ConsigneePostalCode = dto.ConsigneePostalCode,
            ConsigneePhone = dto.ConsigneePhone,
            ShipperName = dto.ShipperName,
            ShipperCountry = dto.ShipperCountry,
            Pieces = dto.Pieces,
            Weight = dto.Weight,
            ContentsDescription = dto.ContentsDescription,
            HSCode = dto.HSCode,
            DeclaredValue = dto.DeclaredValue,
            Currency = dto.Currency,
            SpecialInstructions = dto.SpecialInstructions,
            PaymentMode = ParsePaymentMode(dto.PaymentMode),
            Status = ImportShipmentStatus.Expected,
            CreatedAt = DateTime.UtcNow
        }).ToList();
    }

    private PaymentMode ParsePaymentMode(string? paymentMode)
    {
        if (string.IsNullOrWhiteSpace(paymentMode))
            return PaymentMode.Prepaid;
        
        return paymentMode.ToLower() switch
        {
            "cod" or "collect" => PaymentMode.COD,
            "topay" or "to pay" => PaymentMode.ToPay,
            "credit" => PaymentMode.Credit,
            _ => PaymentMode.Prepaid
        };
    }
}
