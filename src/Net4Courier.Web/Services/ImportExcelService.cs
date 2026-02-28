using ClosedXML.Excel;
using Net4Courier.Kernel.Enums;
using Net4Courier.Masters.Entities;
using Net4Courier.Operations.Entities;

namespace Net4Courier.Web.Services;

public class ImportExcelParseResult
{
    public ImportHeaderDto? Header { get; set; }
    public List<ImportShipmentDto> Shipments { get; set; } = new();
    public List<ImportValidationError> Errors { get; set; } = new();
    public bool IsValid => !Errors.Any(e => e.IsCritical);
    public bool AutoGenerateAwb { get; set; }
}

public class ImportHeaderDto
{
    public ImportMode ImportMode { get; set; } = ImportMode.Air;
    public string MasterReferenceNumber { get; set; } = string.Empty;
    public string? ColoaderNumber { get; set; }
    public string? ColoaderName { get; set; }
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
    public string? ReferenceNo { get; set; }
    public string ConsigneeName { get; set; } = string.Empty;
    public string? ConsigneeAddress { get; set; }
    public string? ConsigneeCity { get; set; }
    public string? ConsigneeState { get; set; }
    public string ConsigneeCountry { get; set; } = string.Empty;
    public string? ConsigneePostalCode { get; set; }
    public string? ConsigneePhone { get; set; }
    public string? ConsigneeMobile2 { get; set; }
    public string? ShipperName { get; set; }
    public string? ShipperAddress { get; set; }
    public string? ShipperCountry { get; set; }
    public string? ServiceType { get; set; }
    public string? CustomerAccountNo { get; set; }
    public int Pieces { get; set; } = 1;
    public decimal Weight { get; set; }
    public string? ContentsDescription { get; set; }
    public string? HSCode { get; set; }
    public decimal? DeclaredValue { get; set; }
    public string? Currency { get; set; }
    public decimal? DutyVatAmount { get; set; }
    public decimal? CodCollectionAmount { get; set; }
    public decimal? AdminChargesShipper { get; set; }
    public decimal? AdminChargesReceiver { get; set; }
    public string? PaymentMode { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? IncoTerms { get; set; }
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
    private static readonly HashSet<string> ValidIncoTermsCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "EXW", "FCA", "CPT", "CIP", "DAP", "DPU", "DDP", "DDU", "FAS", "FOB", "CFR", "CIF"
    };

    private static string? NormalizeIncoTerms(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim().ToUpperInvariant();
        return ValidIncoTermsCodes.Contains(trimmed) ? trimmed : value.Trim();
    }

    public byte[] GenerateTemplate(ImportMode mode)
    {
        return GenerateTemplateWithMasterData(mode, null, null, null, null);
    }
    
    public byte[] GenerateTemplateWithMasterData(ImportMode mode, 
        List<Country>? countries, 
        List<State>? states, 
        List<City>? cities, 
        List<Port>? ports)
    {
        using var workbook = new XLWorkbook();
        
        var headerSheet = workbook.Worksheets.Add("Header");
        CreateHeaderSheet(headerSheet, mode);
        
        var shipmentsSheet = workbook.Worksheets.Add("Shipments");
        CreateShipmentsSheet(shipmentsSheet);
        
        // Add master data tabs if data is provided
        if (countries?.Any() == true)
        {
            var countrySheet = workbook.Worksheets.Add("Countries");
            CreateCountrySheet(countrySheet, countries);
        }
        
        if (states?.Any() == true)
        {
            var stateSheet = workbook.Worksheets.Add("States");
            CreateStateSheet(stateSheet, states, countries);
        }
        
        if (cities?.Any() == true)
        {
            var citySheet = workbook.Worksheets.Add("Cities");
            CreateCitySheet(citySheet, cities);
        }
        
        if (ports?.Any() == true)
        {
            var portSheet = workbook.Worksheets.Add("Ports");
            CreatePortSheet(portSheet, ports, mode);
        }
        
        var incoTermsSheet = workbook.Worksheets.Add("Inco Terms");
        CreateIncoTermsSheet(incoTermsSheet);
        
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
        
        AddHeaderRow(sheet, ref row, "Customer", "", "Yes", "Customer name (must be selected during upload)");
        AddHeaderRow(sheet, ref row, "Transport Mode", mode.ToString(), "Yes", "Air, Sea, or Land");
        AddHeaderRow(sheet, ref row, "Master Reference", "", "Yes", mode == ImportMode.Air ? "MAWB Number" : mode == ImportMode.Sea ? "Bill of Lading Number" : "Truck/Vehicle Number");
        AddHeaderRow(sheet, ref row, "Coloader Number", "", "No", "Co-loader party code from the system");
        AddHeaderRow(sheet, ref row, "Coloader Name", "", "No", "Co-loader party name from the system");
        AddHeaderRow(sheet, ref row, "Origin Country", "", "Yes", "Country of origin (must match system values - see Countries tab)");
        AddHeaderRow(sheet, ref row, "Origin City", "", "No", "City of origin (must match system values - see Cities tab)");
        AddHeaderRow(sheet, ref row, "Origin Port Code", "", "No", mode == ImportMode.Air ? "Origin Airport IATA Code e.g. DXB, LHR (see Ports tab)" : "Origin Port Code e.g. AEJEA (see Ports tab)");
        AddHeaderRow(sheet, ref row, "Destination Country", "", "Yes", "Destination country (must match system values - see Countries tab)");
        AddHeaderRow(sheet, ref row, "Destination City", "", "No", "Destination city (must match system values - see Cities tab)");
        AddHeaderRow(sheet, ref row, "Destination Port Code", "", "No", mode == ImportMode.Air ? "Destination Airport IATA Code e.g. JFK, SIN (see Ports tab)" : "Destination Port Code e.g. USNYC (see Ports tab)");
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
        
        // Add remarks section
        row += 2;
        sheet.Cell(row, 1).Value = "IMPORTANT NOTES:";
        sheet.Cell(row, 1).Style.Font.Bold = true;
        sheet.Cell(row, 1).Style.Font.FontColor = XLColor.Red;
        sheet.Range(row, 1, row, 4).Merge();
        row++;
        
        sheet.Cell(row, 1).Value = "- Origin Country, Origin City, and Origin Airport/Port values must exactly match the system's master data.";
        sheet.Range(row, 1, row, 4).Merge();
        row++;
        
        sheet.Cell(row, 1).Value = "- Destination Country, Destination City, and Destination Airport/Port values must exactly match the system's master data.";
        sheet.Range(row, 1, row, 4).Merge();
        row++;
        
        sheet.Cell(row, 1).Value = "- Refer to the Countries, States, Cities, and Ports tabs for valid system values.";
        sheet.Range(row, 1, row, 4).Merge();
        
        sheet.Columns().AdjustToContents();
        sheet.Column(2).Width = 30;
        sheet.Column(4).Width = 50;
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
        
        // Add description for AWB column
        sheet.Cell(2, 1).Value = "Leave blank if using Auto-Generate AWB option during upload";
        sheet.Cell(2, 1).Style.Font.Italic = true;
        sheet.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
        sheet.Range(2, 1, 2, 5).Merge();
        
        int col = 1;
        var headers = new[]
        {
            ("AWB No", false),
            ("Ref. AWB No.", false),
            ("Consignee Name *", true),
            ("Consignee Address", false),
            ("Consignee City", false),
            ("Consignee State", false),
            ("Consignee Country *", true),
            ("Consignee Postal Code", false),
            ("Consignee Phone", false),
            ("Secondary Mobile", false),
            ("Shipper Name", false),
            ("Shipper Address", false),
            ("Shipper Country", false),
            ("Service Type", false),
            ("Customer Account No", false),
            ("Pieces *", true),
            ("Weight (kg) *", true),
            ("Contents Description", false),
            ("HS Code", false),
            ("Declared Value", false),
            ("Currency", false),
            ("Duty/VAT Amount", false),
            ("COD/Collection Amount", false),
            ("Admin Charges-Shipper", false),
            ("Admin Charges-Receiver", false),
            ("Payment Mode", false),
            ("Inco Terms", false),
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
    
    private void CreateCountrySheet(IXLWorksheet sheet, List<Country> countries)
    {
        sheet.Cell(1, 1).Value = "Countries - Master Data (Reference Only)";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 3).Merge();
        
        int row = 3;
        sheet.Cell(row, 1).Value = "Code";
        sheet.Cell(row, 2).Value = "Name";
        sheet.Cell(row, 3).Value = "IATA Code";
        sheet.Range(row, 1, row, 3).Style.Font.Bold = true;
        sheet.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;
        
        foreach (var country in countries.OrderBy(c => c.Name))
        {
            sheet.Cell(row, 1).Value = country.Code ?? "";
            sheet.Cell(row, 2).Value = country.Name;
            sheet.Cell(row, 3).Value = country.IATACode ?? "";
            row++;
        }
        
        sheet.Columns().AdjustToContents();
    }
    
    private void CreateStateSheet(IXLWorksheet sheet, List<State> states, List<Country>? countries)
    {
        sheet.Cell(1, 1).Value = "States - Master Data (Reference Only)";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 3).Merge();
        
        int row = 3;
        sheet.Cell(row, 1).Value = "Code";
        sheet.Cell(row, 2).Value = "Name";
        sheet.Cell(row, 3).Value = "Country";
        sheet.Range(row, 1, row, 3).Style.Font.Bold = true;
        sheet.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;
        
        var countryDict = countries?.ToDictionary(c => c.Id, c => c.Name) ?? new Dictionary<long, string>();
        
        foreach (var state in states.OrderBy(s => s.Name))
        {
            sheet.Cell(row, 1).Value = state.Code ?? "";
            sheet.Cell(row, 2).Value = state.Name;
            sheet.Cell(row, 3).Value = countryDict.ContainsKey(state.CountryId) 
                ? countryDict[state.CountryId] : "";
            row++;
        }
        
        sheet.Columns().AdjustToContents();
    }
    
    private void CreateCitySheet(IXLWorksheet sheet, List<City> cities)
    {
        sheet.Cell(1, 1).Value = "Cities - Master Data (Reference Only)";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 3).Merge();
        
        int row = 3;
        sheet.Cell(row, 1).Value = "Code";
        sheet.Cell(row, 2).Value = "Name";
        sheet.Cell(row, 3).Value = "Is Hub";
        sheet.Range(row, 1, row, 3).Style.Font.Bold = true;
        sheet.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;
        
        foreach (var city in cities.OrderBy(c => c.Name))
        {
            sheet.Cell(row, 1).Value = city.Code;
            sheet.Cell(row, 2).Value = city.Name;
            sheet.Cell(row, 3).Value = city.IsHub ? "Yes" : "No";
            row++;
        }
        
        sheet.Columns().AdjustToContents();
    }
    
    private void CreatePortSheet(IXLWorksheet sheet, List<Port> ports, ImportMode mode)
    {
        var portType = mode == ImportMode.Air ? "Airports" : "Ports";
        sheet.Cell(1, 1).Value = $"{portType} - Master Data (Reference Only)";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 4).Merge();
        
        int row = 3;
        sheet.Cell(row, 1).Value = "Code";
        sheet.Cell(row, 2).Value = "IATA Code";
        sheet.Cell(row, 3).Value = "Name";
        sheet.Cell(row, 4).Value = "Type";
        sheet.Range(row, 1, row, 4).Style.Font.Bold = true;
        sheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;
        
        foreach (var port in ports.OrderBy(p => p.Name))
        {
            sheet.Cell(row, 1).Value = port.Code;
            sheet.Cell(row, 2).Value = port.IATACode ?? "";
            sheet.Cell(row, 3).Value = port.Name;
            sheet.Cell(row, 4).Value = port.PortType.ToString();
            row++;
        }
        
        sheet.Columns().AdjustToContents();
    }

    private void CreateIncoTermsSheet(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "Inco Terms - Reference Data";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 3).Merge();

        int row = 3;
        sheet.Cell(row, 1).Value = "Code";
        sheet.Cell(row, 2).Value = "Full Name";
        sheet.Cell(row, 3).Value = "Description";
        sheet.Range(row, 1, row, 3).Style.Font.Bold = true;
        sheet.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;

        var incoTerms = new[]
        {
            ("EXW", "Ex Works", "Seller makes goods available at their premises"),
            ("FCA", "Free Carrier", "Seller delivers goods to carrier nominated by buyer"),
            ("CPT", "Carriage Paid To", "Seller pays freight to named destination"),
            ("CIP", "Carriage and Insurance Paid To", "Seller pays freight and insurance to named destination"),
            ("DAP", "Delivered at Place", "Seller delivers goods at named place of destination"),
            ("DPU", "Delivered at Place Unloaded", "Seller delivers and unloads goods at named place"),
            ("DDP", "Delivered Duty Paid", "Seller bears all costs and duties to destination"),
            ("DDU", "Delivered Duty Unpaid", "Seller delivers goods but buyer pays duties and taxes"),
            ("FAS", "Free Alongside Ship", "Seller delivers goods alongside the vessel (sea only)"),
            ("FOB", "Free on Board", "Seller delivers goods on board the vessel (sea only)"),
            ("CFR", "Cost and Freight", "Seller pays costs and freight to destination port (sea only)"),
            ("CIF", "Cost, Insurance and Freight", "Seller pays costs, insurance and freight to destination port (sea only)")
        };

        foreach (var (code, fullName, description) in incoTerms)
        {
            sheet.Cell(row, 1).Value = code;
            sheet.Cell(row, 2).Value = fullName;
            sheet.Cell(row, 3).Value = description;
            row++;
        }

        sheet.Columns().AdjustToContents();
        sheet.Column(3).Width = 55;
    }

    public ImportExcelParseResult ParseExcel(Stream stream, bool autoGenerateAwb = false)
    {
        var result = new ImportExcelParseResult();
        result.AutoGenerateAwb = autoGenerateAwb;
        
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
            
            result.Shipments = ParseShipments(shipmentsSheet, result.Errors, autoGenerateAwb);
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
        
        if (fieldValues.TryGetValue("Coloader Number", out var coloaderNumber))
            header.ColoaderNumber = coloaderNumber;
        if (fieldValues.TryGetValue("Coloader Name", out var coloaderName))
            header.ColoaderName = coloaderName;
        
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
        if (fieldValues.TryGetValue("Origin Port Code", out var originPort) || 
            fieldValues.TryGetValue("Origin Port/Airport", out originPort))
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
        if (fieldValues.TryGetValue("Destination Port Code", out var destPort) || 
            fieldValues.TryGetValue("Destination Port/Airport", out destPort))
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
        
        ValidateHeaderFieldLengths(header, errors);
        
        return header;
    }
    
    private void ValidateHeaderFieldLengths(ImportHeaderDto header, List<ImportValidationError> errors)
    {
        var fieldLimits = new Dictionary<string, (string? Value, int MaxLength)>
        {
            { "Master Reference", (header.MasterReferenceNumber, 50) },
            { "Co-Loader Reference", (header.ColoaderNumber, 50) },
            { "Co-Loader Name", (header.ColoaderName, 100) },
            { "Origin Country", (header.OriginCountry, 100) },
            { "Origin City", (header.OriginCity, 100) },
            { "Origin Port Code", (header.OriginPort, 20) },
            { "Destination Country", (header.DestinationCountry, 100) },
            { "Destination City", (header.DestinationCity, 100) },
            { "Destination Port Code", (header.DestinationPort, 20) },
            { "Carrier Name", (header.CarrierName, 100) },
            { "Carrier Code", (header.CarrierCode, 20) },
            { "Flight No", (header.FlightNo, 20) },
            { "Vessel Name", (header.VesselName, 100) },
            { "Voyage Number", (header.VoyageNumber, 50) },
            { "Truck Number", (header.TruckNumber, 50) },
            { "Driver Name", (header.DriverName, 100) },
            { "Driver Phone", (header.DriverPhone, 20) },
            { "Manifest Number", (header.ManifestNumber, 50) }
        };
        
        foreach (var field in fieldLimits)
        {
            if (!string.IsNullOrEmpty(field.Value.Value) && field.Value.Value.Length > field.Value.MaxLength)
            {
                errors.Add(new ImportValidationError
                {
                    Sheet = "Header",
                    Column = field.Key,
                    Message = $"{field.Key}: value exceeds {field.Value.MaxLength} characters (current: {field.Value.Value.Length})",
                    IsCritical = true
                });
            }
        }
    }
    
    private void ValidateShipmentFieldLengths(ImportShipmentDto shipment, int row, List<ImportValidationError> errors)
    {
        var fieldLimits = new Dictionary<string, (string? Value, int MaxLength)>
        {
            { "AWB No", (shipment.AWBNo, 50) },
            { "Reference No", (shipment.ReferenceNo, 50) },
            { "Consignee Name", (shipment.ConsigneeName, 200) },
            { "Consignee Address", (shipment.ConsigneeAddress, 500) },
            { "Consignee City", (shipment.ConsigneeCity, 100) },
            { "Consignee State", (shipment.ConsigneeState, 100) },
            { "Consignee Country", (shipment.ConsigneeCountry, 100) },
            { "Consignee Postal Code", (shipment.ConsigneePostalCode, 20) },
            { "Consignee Phone", (shipment.ConsigneePhone, 50) },
            { "Secondary Mobile", (shipment.ConsigneeMobile2, 50) },
            { "Shipper Name", (shipment.ShipperName, 200) },
            { "Shipper Address", (shipment.ShipperAddress, 500) },
            { "Shipper Country", (shipment.ShipperCountry, 100) },
            { "Service Type", (shipment.ServiceType, 100) },
            { "Customer Account No", (shipment.CustomerAccountNo, 50) },
            { "Contents Description", (shipment.ContentsDescription, 500) },
            { "HS Code", (shipment.HSCode, 20) },
            { "Currency", (shipment.Currency, 10) },
            { "Special Instructions", (shipment.SpecialInstructions, 500) }
        };
        
        foreach (var field in fieldLimits)
        {
            if (!string.IsNullOrEmpty(field.Value.Value) && field.Value.Value.Length > field.Value.MaxLength)
            {
                errors.Add(new ImportValidationError
                {
                    Sheet = "Shipments",
                    RowNumber = row,
                    Column = field.Key,
                    Message = $"{field.Key}: value exceeds {field.Value.MaxLength} characters (current: {field.Value.Value.Length})",
                    IsCritical = true
                });
            }
        }
    }

    private List<ImportShipmentDto> ParseShipments(IXLWorksheet sheet, List<ImportValidationError> errors, bool autoGenerateAwb = false)
    {
        var shipments = new List<ImportShipmentDto>();
        
        // Find the header row and column positions dynamically
        int headerRow = 3;
        var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        
        // Search for header row by looking for "Consignee Name" in any column
        for (int r = 1; r <= 5; r++)
        {
            for (int c = 1; c <= 25; c++)
            {
                var cell = sheet.Cell(r, c).GetString()?.Trim();
                if (cell != null && cell.Contains("Consignee Name", StringComparison.OrdinalIgnoreCase))
                {
                    headerRow = r;
                    break;
                }
            }
            if (headerRow == r) break;
        }
        
        // Build column map from header row
        for (int c = 1; c <= 25; c++)
        {
            var header = sheet.Cell(headerRow, c).GetString()?.Trim();
            if (!string.IsNullOrWhiteSpace(header))
            {
                // Normalize header names for matching
                var normalizedHeader = header.Replace("*", "").Replace("(kg)", "").Trim();
                if (!columnMap.ContainsKey(normalizedHeader))
                    columnMap[normalizedHeader] = c;
            }
        }
        
        // Helper to get column index by header name
        int GetCol(params string[] names)
        {
            foreach (var name in names)
            {
                if (columnMap.TryGetValue(name, out var col)) return col;
            }
            return -1;
        }
        
        // Map columns
        int colAwb = GetCol("AWB No", "AWB");
        int colRef = GetCol("Ref. AWB No.", "Ref AWB No", "Reference No");
        int colConsigneeName = GetCol("Consignee Name");
        int colConsigneeAddr = GetCol("Consignee Address");
        int colConsigneeCity = GetCol("Consignee City");
        int colConsigneeState = GetCol("Consignee State");
        int colConsigneeCountry = GetCol("Consignee Country");
        int colConsigneePostal = GetCol("Consignee Postal Code");
        int colConsigneePhone = GetCol("Consignee Phone");
        int colSecondaryMobile = GetCol("Secondary Mobile", "Mobile 2", "ConsigneeMobile2");
        int colShipperName = GetCol("Shipper Name");
        int colShipperAddress = GetCol("Shipper Address");
        int colShipperCountry = GetCol("Shipper Country");
        int colServiceType = GetCol("Service Type", "ServiceType");
        int colCustomerAccountNo = GetCol("Customer Account No", "Customer Account", "Account No");
        int colPieces = GetCol("Pieces");
        int colWeight = GetCol("Weight");
        int colContents = GetCol("Contents Description");
        int colHsCode = GetCol("HS Code");
        int colDeclaredVal = GetCol("Declared Value");
        int colCurrency = GetCol("Currency");
        int colDutyVat = GetCol("Duty/VAT Amount", "Duty VAT Amount");
        int colCodColl = GetCol("COD/Collection Amount", "COD Collection Amount");
        int colAdminShipper = GetCol("Admin Charges-Shipper", "Admin Charges Shipper");
        int colAdminReceiver = GetCol("Admin Charges-Receiver", "Admin Charges Receiver");
        int colPaymentMode = GetCol("Payment Mode");
        int colIncoTerms = GetCol("Inco Terms", "IncoTerms", "Incoterms");
        int colSpecialInstr = GetCol("Special Instructions");
        
        int row = headerRow + 1; // Data starts after header row
        int emptyRowCount = 0;
        while (true)
        {
            var awbNo = colAwb > 0 ? sheet.Cell(row, colAwb).GetString()?.Trim() : null;
            var consigneeName = colConsigneeName > 0 ? sheet.Cell(row, colConsigneeName).GetString()?.Trim() : null;
            
            // Use consignee name to detect rows with data (works for both auto-generate modes)
            var hasData = !string.IsNullOrWhiteSpace(consigneeName) || !string.IsNullOrWhiteSpace(awbNo);
            
            if (!hasData)
            {
                emptyRowCount++;
                if (emptyRowCount >= 3) // Stop after 3 consecutive empty rows
                    break;
                row++;
                continue;
            }
            emptyRowCount = 0; // Reset counter when we find data
            
            var shipment = new ImportShipmentDto
            {
                RowNumber = row,
                AWBNo = awbNo ?? "",
                ReferenceNo = colRef > 0 ? sheet.Cell(row, colRef).GetString()?.Trim() : null,
                ConsigneeName = consigneeName ?? "",
                ConsigneeAddress = colConsigneeAddr > 0 ? sheet.Cell(row, colConsigneeAddr).GetString()?.Trim() : null,
                ConsigneeCity = colConsigneeCity > 0 ? sheet.Cell(row, colConsigneeCity).GetString()?.Trim() : null,
                ConsigneeState = colConsigneeState > 0 ? sheet.Cell(row, colConsigneeState).GetString()?.Trim() : null,
                ConsigneeCountry = colConsigneeCountry > 0 ? sheet.Cell(row, colConsigneeCountry).GetString()?.Trim() ?? "" : "",
                ConsigneePostalCode = colConsigneePostal > 0 ? sheet.Cell(row, colConsigneePostal).GetString()?.Trim() : null,
                ConsigneePhone = colConsigneePhone > 0 ? sheet.Cell(row, colConsigneePhone).GetString()?.Trim() : null,
                ConsigneeMobile2 = colSecondaryMobile > 0 ? sheet.Cell(row, colSecondaryMobile).GetString()?.Trim() : null,
                ShipperName = colShipperName > 0 ? sheet.Cell(row, colShipperName).GetString()?.Trim() : null,
                ShipperAddress = colShipperAddress > 0 ? sheet.Cell(row, colShipperAddress).GetString()?.Trim() : null,
                ShipperCountry = colShipperCountry > 0 ? sheet.Cell(row, colShipperCountry).GetString()?.Trim() : null,
                ServiceType = colServiceType > 0 ? sheet.Cell(row, colServiceType).GetString()?.Trim() : null,
                CustomerAccountNo = colCustomerAccountNo > 0 ? sheet.Cell(row, colCustomerAccountNo).GetString()?.Trim() : null,
                ContentsDescription = colContents > 0 ? sheet.Cell(row, colContents).GetString()?.Trim() : null,
                HSCode = colHsCode > 0 ? sheet.Cell(row, colHsCode).GetString()?.Trim() : null,
                Currency = colCurrency > 0 ? sheet.Cell(row, colCurrency).GetString()?.Trim() : null,
                PaymentMode = colPaymentMode > 0 ? sheet.Cell(row, colPaymentMode).GetString()?.Trim() : null,
                IncoTerms = colIncoTerms > 0 ? NormalizeIncoTerms(sheet.Cell(row, colIncoTerms).GetString()?.Trim()) : null,
                SpecialInstructions = colSpecialInstr > 0 ? sheet.Cell(row, colSpecialInstr).GetString()?.Trim() : null
            };
            
            if (colPieces > 0)
            {
                var piecesVal = sheet.Cell(row, colPieces).Value;
                if (piecesVal.IsNumber)
                    shipment.Pieces = (int)piecesVal.GetNumber();
                else if (int.TryParse(sheet.Cell(row, colPieces).GetString(), out var pieces))
                    shipment.Pieces = pieces;
                else
                    shipment.Pieces = 1;
            }
            else
            {
                shipment.Pieces = 1;
            }
            
            if (colWeight > 0)
            {
                var weightVal = sheet.Cell(row, colWeight).Value;
                if (weightVal.IsNumber)
                    shipment.Weight = (decimal)weightVal.GetNumber();
                else if (decimal.TryParse(sheet.Cell(row, colWeight).GetString(), out var weight))
                    shipment.Weight = weight;
            }
            
            if (colDeclaredVal > 0)
            {
                var declaredVal = sheet.Cell(row, colDeclaredVal).Value;
                if (declaredVal.IsNumber)
                    shipment.DeclaredValue = (decimal)declaredVal.GetNumber();
                else if (decimal.TryParse(sheet.Cell(row, colDeclaredVal).GetString(), out var declaredValue))
                    shipment.DeclaredValue = declaredValue;
            }
            
            if (colDutyVat > 0)
            {
                var dutyVatVal = sheet.Cell(row, colDutyVat).Value;
                if (dutyVatVal.IsNumber)
                    shipment.DutyVatAmount = (decimal)dutyVatVal.GetNumber();
                else if (decimal.TryParse(sheet.Cell(row, colDutyVat).GetString(), out var dutyVat))
                    shipment.DutyVatAmount = dutyVat;
            }
            
            if (colCodColl > 0)
            {
                var codCollVal = sheet.Cell(row, colCodColl).Value;
                if (codCollVal.IsNumber)
                    shipment.CodCollectionAmount = (decimal)codCollVal.GetNumber();
                else if (decimal.TryParse(sheet.Cell(row, colCodColl).GetString(), out var codColl))
                    shipment.CodCollectionAmount = codColl;
            }
            
            if (colAdminShipper > 0)
            {
                var adminShipperVal = sheet.Cell(row, colAdminShipper).Value;
                if (adminShipperVal.IsNumber)
                    shipment.AdminChargesShipper = (decimal)adminShipperVal.GetNumber();
                else if (decimal.TryParse(sheet.Cell(row, colAdminShipper).GetString(), out var adminShipper))
                    shipment.AdminChargesShipper = adminShipper;
            }
            
            if (colAdminReceiver > 0)
            {
                var adminReceiverVal = sheet.Cell(row, colAdminReceiver).Value;
                if (adminReceiverVal.IsNumber)
                    shipment.AdminChargesReceiver = (decimal)adminReceiverVal.GetNumber();
                else if (decimal.TryParse(sheet.Cell(row, colAdminReceiver).GetString(), out var adminReceiver))
                    shipment.AdminChargesReceiver = adminReceiver;
            }
            
            // Check for missing AWB when auto-generate is disabled
            if (!autoGenerateAwb && string.IsNullOrWhiteSpace(shipment.AWBNo))
            {
                errors.Add(new ImportValidationError 
                { 
                    Sheet = "Shipments", 
                    RowNumber = row, 
                    Column = "AWB No", 
                    Message = "AWB number is required when auto-generate is disabled",
                    IsCritical = true
                });
            }
            
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
            
            ValidateShipmentFieldLengths(shipment, row, errors);
            
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
        
        foreach (var s in shipments)
        {
            if (!string.IsNullOrWhiteSpace(s.AWBNo))
                s.AWBNo = s.AWBNo.Trim().ToUpperInvariant();
        }
        
        if (!autoGenerateAwb)
        {
            var duplicateAwbs = shipments
                .Where(s => !string.IsNullOrWhiteSpace(s.AWBNo))
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
        }
        
        var invalidIncoTerms = shipments
            .Where(s => !string.IsNullOrWhiteSpace(s.IncoTerms) && !ValidIncoTermsCodes.Contains(s.IncoTerms.ToUpperInvariant()))
            .ToList();
        foreach (var inv in invalidIncoTerms)
        {
            errors.Add(new ImportValidationError
            {
                Sheet = "Shipments",
                RowNumber = inv.RowNumber,
                Column = "Inco Terms",
                Message = $"Invalid Inco Terms code '{inv.IncoTerms}' in row {inv.RowNumber}. Valid codes: {string.Join(", ", ValidIncoTermsCodes)}"
            });
        }
        
        return shipments;
    }

    public ImportMaster CreateImportMaster(ImportHeaderDto header, long? branchId, long? companyId, long? financialYearId, ShipmentDirection shipmentDirection = ShipmentDirection.Import)
    {
        var dirPrefix = shipmentDirection == ShipmentDirection.Export ? "EXP" : "IMP";
        var modePrefix = header.ImportMode switch
        {
            ImportMode.Air => "AIR",
            ImportMode.Sea => "SEA",
            ImportMode.Land => "LND",
            _ => ""
        };
        var refPrefix = $"{dirPrefix}-{modePrefix}";
        
        return new ImportMaster
        {
            ImportRefNo = $"{refPrefix}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            TransactionDate = DateTime.UtcNow,
            FinancialYearId = financialYearId,
            CompanyId = companyId,
            BranchId = branchId,
            ImportMode = header.ImportMode,
            ShipmentDirection = shipmentDirection,
            MasterReferenceType = header.ImportMode == ImportMode.Air ? MasterReferenceType.MAWB : 
                                  header.ImportMode == ImportMode.Sea ? MasterReferenceType.BL : MasterReferenceType.TruckWaybill,
            MasterReferenceNumber = header.MasterReferenceNumber,
            CoLoaderRefNo = header.ColoaderNumber,
            CoLoaderName = header.ColoaderName,
            OriginCountryName = header.OriginCountry,
            OriginCityName = header.OriginCity,
            OriginPortCode = header.OriginPort,
            DestinationCountryName = header.DestinationCountry,
            DestinationCityName = header.DestinationCity,
            DestinationPortCode = header.DestinationPort,
            ETD = ToUtc(header.ETD),
            ETA = ToUtc(header.ETA),
            CarrierName = header.CarrierName,
            CarrierCode = header.CarrierCode,
            FlightNo = header.FlightNo,
            FlightDate = ToUtc(header.FlightDate),
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
            ReferenceNo = dto.ReferenceNo,
            ConsigneeName = dto.ConsigneeName,
            ConsigneeAddress = dto.ConsigneeAddress,
            ConsigneeCity = dto.ConsigneeCity,
            ConsigneeState = dto.ConsigneeState,
            ConsigneeCountry = dto.ConsigneeCountry,
            ConsigneePostalCode = dto.ConsigneePostalCode,
            ConsigneePhone = dto.ConsigneePhone,
            ConsigneeMobile2 = dto.ConsigneeMobile2,
            ShipperName = dto.ShipperName,
            ShipperAddress = dto.ShipperAddress,
            ShipperCountry = dto.ShipperCountry,
            ServiceType = dto.ServiceType,
            CustomerAccountNo = dto.CustomerAccountNo,
            Pieces = dto.Pieces,
            Weight = dto.Weight,
            ContentsDescription = dto.ContentsDescription,
            HSCode = dto.HSCode,
            DeclaredValue = dto.DeclaredValue,
            Currency = dto.Currency,
            DutyAmount = dto.DutyVatAmount,
            CODAmount = dto.CodCollectionAmount,
            IsCOD = dto.CodCollectionAmount.HasValue && dto.CodCollectionAmount > 0,
            AdminChargesShipper = dto.AdminChargesShipper,
            AdminChargesReceiver = dto.AdminChargesReceiver,
            SpecialInstructions = dto.SpecialInstructions,
            IncoTerms = dto.IncoTerms,
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
            "account" => PaymentMode.Account,
            "pickup cash" or "pickupcash" or "cash" => PaymentMode.PickupCash,
            _ => PaymentMode.Prepaid
        };
    }

    private static DateTime? ToUtc(DateTime? dateTime)
    {
        if (!dateTime.HasValue) return null;
        var dt = dateTime.Value;
        return dt.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) 
            : dt.ToUniversalTime();
    }
}
