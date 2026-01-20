using ClosedXML.Excel;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class PODExcelRowDto
{
    public int RowNumber { get; set; }
    public string AwbNo { get; set; } = "";
    public string? DeliveryStatus { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Relation { get; set; }
    public string? NonDeliveryReason { get; set; }
    public string? Remarks { get; set; }
}

public class PODExcelParseResult
{
    public List<PODExcelRowDto> Rows { get; set; } = new();
    public List<PODValidationError> Errors { get; set; } = new();
    public bool IsValid => !Errors.Any(e => e.IsCritical);
}

public class PODValidationError
{
    public int? RowNumber { get; set; }
    public string? Column { get; set; }
    public string Message { get; set; } = "";
    public bool IsCritical { get; set; } = true;
}

public class PODExcelService
{
    public byte[] GenerateTemplate(List<(string AwbNo, string Consignee, string Destination, string Status)>? awbs = null)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("POD Updates");

        sheet.Cell(1, 1).Value = "POD Batch Update Template";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;
        sheet.Range(1, 1, 1, 6).Merge();

        int row = 3;
        var headers = new[]
        {
            ("AWB No *", true, 15),
            ("Delivery Status *", true, 18),
            ("Delivery Date *", true, 15),
            ("Received By", false, 20),
            ("Relation", false, 15),
            ("Non-Delivery Reason", false, 25),
            ("Remarks", false, 30),
            ("Consignee (Info Only)", false, 25),
            ("Destination (Info Only)", false, 20),
            ("Current Status (Info Only)", false, 20)
        };

        int col = 1;
        foreach (var (header, required, width) in headers)
        {
            sheet.Cell(row, col).Value = header;
            sheet.Cell(row, col).Style.Font.Bold = true;
            sheet.Cell(row, col).Style.Fill.BackgroundColor = required ? XLColor.LightBlue : XLColor.LightGray;
            sheet.Column(col).Width = width;
            col++;
        }

        sheet.Range(row, 1, row, headers.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        sheet.Range(row, 1, row, headers.Length).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        row++;

        if (awbs != null && awbs.Count > 0)
        {
            foreach (var awb in awbs)
            {
                sheet.Cell(row, 1).Value = awb.AwbNo;
                sheet.Cell(row, 2).Value = "";
                sheet.Cell(row, 3).Value = "";
                sheet.Cell(row, 4).Value = "";
                sheet.Cell(row, 5).Value = "";
                sheet.Cell(row, 6).Value = "";
                sheet.Cell(row, 7).Value = "";
                sheet.Cell(row, 8).Value = awb.Consignee;
                sheet.Cell(row, 9).Value = awb.Destination;
                sheet.Cell(row, 10).Value = awb.Status;

                sheet.Range(row, 8, row, 10).Style.Fill.BackgroundColor = XLColor.LightGoldenrodYellow;
                sheet.Range(row, 8, row, 10).Style.Font.FontColor = XLColor.DarkGray;

                row++;
            }
        }

        var instructionsSheet = workbook.Worksheets.Add("Instructions");
        AddInstructions(instructionsSheet);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void AddInstructions(IXLWorksheet sheet)
    {
        sheet.Cell(1, 1).Value = "Instructions for POD Batch Update";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;

        int row = 3;
        
        sheet.Cell(row++, 1).Value = "Required Fields:";
        sheet.Cell(row - 1, 1).Style.Font.Bold = true;
        sheet.Cell(row++, 1).Value = "- AWB No: The airway bill number";
        sheet.Cell(row++, 1).Value = "- Delivery Status: DELIVERED, PARTIAL, REFUSED, or NOT DELIVERED";
        sheet.Cell(row++, 1).Value = "- Delivery Date: Date of delivery (DD/MM/YYYY or YYYY-MM-DD format)";

        row++;
        sheet.Cell(row++, 1).Value = "For Delivered/Partial Status:";
        sheet.Cell(row - 1, 1).Style.Font.Bold = true;
        sheet.Cell(row++, 1).Value = "- Received By: Name of person who received the shipment";
        sheet.Cell(row++, 1).Value = "- Relation: SELF, FAMILY, COLLEAGUE, SECURITY, RECEPTION, NEIGHBOR, or OTHER";

        row++;
        sheet.Cell(row++, 1).Value = "For Refused/Not Delivered Status:";
        sheet.Cell(row - 1, 1).Style.Font.Bold = true;
        sheet.Cell(row++, 1).Value = "- Non-Delivery Reason: ADDRESS NOT FOUND, CUSTOMER NOT AVAILABLE, REFUSED, PREMISES CLOSED,";
        sheet.Cell(row++, 1).Value = "  INCORRECT ADDRESS, RESCHEDULE, WEATHER, ACCESS RESTRICTED, or OTHER";

        row++;
        sheet.Cell(row++, 1).Value = "Delivery Status Values:";
        sheet.Cell(row - 1, 1).Style.Font.Bold = true;
        sheet.Cell(row++, 1).Value = "DELIVERED - Successfully delivered";
        sheet.Cell(row++, 1).Value = "PARTIAL - Partial delivery (some items delivered)";
        sheet.Cell(row++, 1).Value = "REFUSED - Receiver refused delivery";
        sheet.Cell(row++, 1).Value = "NOT DELIVERED - Could not deliver (various reasons)";

        row++;
        sheet.Cell(row++, 1).Value = "Notes:";
        sheet.Cell(row - 1, 1).Style.Font.Bold = true;
        sheet.Cell(row++, 1).Value = "- Columns marked 'Info Only' are for reference and will not be processed";
        sheet.Cell(row++, 1).Value = "- Maximum 500 rows per upload";
        sheet.Cell(row++, 1).Value = "- AWBs that are already delivered or cancelled will be skipped";

        sheet.Columns().AdjustToContents();
    }

    public PODExcelParseResult ParseExcel(Stream stream)
    {
        var result = new PODExcelParseResult();

        try
        {
            using var workbook = new XLWorkbook(stream);
            
            var sheet = workbook.Worksheet("POD Updates");
            if (sheet == null)
            {
                sheet = workbook.Worksheets.FirstOrDefault();
                if (sheet == null)
                {
                    result.Errors.Add(new PODValidationError
                    {
                        Message = "No worksheets found in the Excel file"
                    });
                    return result;
                }
            }

            int row = 4;
            int processedCount = 0;

            while (processedCount < 500)
            {
                var awbNo = sheet.Cell(row, 1).GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(awbNo))
                {
                    var nextRowAwb = sheet.Cell(row + 1, 1).GetString()?.Trim();
                    if (string.IsNullOrWhiteSpace(nextRowAwb))
                        break;
                    row++;
                    continue;
                }

                var podRow = new PODExcelRowDto
                {
                    RowNumber = row,
                    AwbNo = awbNo,
                    DeliveryStatus = sheet.Cell(row, 2).GetString()?.Trim(),
                    ReceivedBy = sheet.Cell(row, 4).GetString()?.Trim(),
                    Relation = sheet.Cell(row, 5).GetString()?.Trim(),
                    NonDeliveryReason = sheet.Cell(row, 6).GetString()?.Trim(),
                    Remarks = sheet.Cell(row, 7).GetString()?.Trim()
                };

                var dateVal = sheet.Cell(row, 3).Value;
                if (dateVal.IsDateTime)
                {
                    podRow.DeliveryDate = dateVal.GetDateTime();
                }
                else
                {
                    var dateStr = sheet.Cell(row, 3).GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(dateStr))
                    {
                        if (DateTime.TryParse(dateStr, out var parsedDate))
                        {
                            podRow.DeliveryDate = parsedDate;
                        }
                        else
                        {
                            result.Errors.Add(new PODValidationError
                            {
                                RowNumber = row,
                                Column = "Delivery Date",
                                Message = $"Invalid date format: {dateStr}",
                                IsCritical = false
                            });
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(podRow.DeliveryStatus))
                {
                    result.Errors.Add(new PODValidationError
                    {
                        RowNumber = row,
                        Column = "Delivery Status",
                        Message = "Delivery Status is required"
                    });
                }
                else if (PODUpdateService.ParseDeliveryStatus(podRow.DeliveryStatus) == null)
                {
                    result.Errors.Add(new PODValidationError
                    {
                        RowNumber = row,
                        Column = "Delivery Status",
                        Message = $"Invalid Delivery Status: {podRow.DeliveryStatus}. Valid values: DELIVERED, PARTIAL, REFUSED, NOT DELIVERED"
                    });
                }

                if (!podRow.DeliveryDate.HasValue)
                {
                    result.Errors.Add(new PODValidationError
                    {
                        RowNumber = row,
                        Column = "Delivery Date",
                        Message = "Delivery Date is required"
                    });
                }

                result.Rows.Add(podRow);
                row++;
                processedCount++;
            }

            if (result.Rows.Count == 0)
            {
                result.Errors.Add(new PODValidationError
                {
                    Message = "No POD records found in the Excel file"
                });
            }

            var duplicateAwbs = result.Rows
                .GroupBy(r => r.AwbNo.ToUpperInvariant())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            foreach (var dup in duplicateAwbs)
            {
                var dupRows = result.Rows.Where(r => r.AwbNo.ToUpperInvariant() == dup).Select(r => r.RowNumber);
                result.Errors.Add(new PODValidationError
                {
                    Column = "AWB No",
                    Message = $"Duplicate AWB '{dup}' found in rows {string.Join(", ", dupRows)}"
                });
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add(new PODValidationError
            {
                Message = $"Error reading Excel file: {ex.Message}"
            });
        }

        return result;
    }

    public byte[] GenerateErrorReport(List<PODUpdateResult> results)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Results");

        sheet.Cell(1, 1).Value = "POD Batch Update Results";
        sheet.Cell(1, 1).Style.Font.Bold = true;
        sheet.Cell(1, 1).Style.Font.FontSize = 14;

        int successCount = results.Count(r => r.Success);
        int failCount = results.Count(r => !r.Success);

        sheet.Cell(2, 1).Value = $"Success: {successCount}, Failed: {failCount}, Total: {results.Count}";

        sheet.Cell(4, 1).Value = "AWB No";
        sheet.Cell(4, 2).Value = "Status";
        sheet.Cell(4, 3).Value = "Error Message";

        sheet.Range(4, 1, 4, 3).Style.Font.Bold = true;
        sheet.Range(4, 1, 4, 3).Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 5;
        foreach (var result in results)
        {
            sheet.Cell(row, 1).Value = result.AwbNo;
            sheet.Cell(row, 2).Value = result.Success ? "Success" : "Failed";
            sheet.Cell(row, 3).Value = result.ErrorMessage ?? "";

            if (result.Success)
            {
                sheet.Cell(row, 2).Style.Font.FontColor = XLColor.Green;
            }
            else
            {
                sheet.Cell(row, 2).Style.Font.FontColor = XLColor.Red;
                sheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightPink;
            }

            row++;
        }

        sheet.Columns().AdjustToContents();
        sheet.Column(3).Width = 50;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
