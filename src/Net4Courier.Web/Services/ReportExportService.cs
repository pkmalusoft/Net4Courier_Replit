using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Net4Courier.Web.Services;

public class ReportExportService
{
    public byte[] ExportToExcel<T>(string sheetName, IEnumerable<T> data, Dictionary<string, Func<T, object?>> columns)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(sheetName);

        int col = 1;
        foreach (var column in columns)
        {
            sheet.Cell(1, col).Value = column.Key;
            sheet.Cell(1, col).Style.Font.Bold = true;
            sheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
            col++;
        }

        int row = 2;
        foreach (var item in data)
        {
            col = 1;
            foreach (var column in columns)
            {
                var value = column.Value(item);
                if (value is decimal d)
                    sheet.Cell(row, col).Value = d;
                else if (value is int i)
                    sheet.Cell(row, col).Value = i;
                else if (value is long l)
                    sheet.Cell(row, col).Value = l;
                else if (value is DateTime dt)
                    sheet.Cell(row, col).Value = dt.ToString("dd-MMM-yyyy");
                else
                    sheet.Cell(row, col).Value = value?.ToString() ?? "";
                col++;
            }
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportToPdf<T>(string title, string subtitle, IEnumerable<T> data, Dictionary<string, Func<T, object?>> columns, Dictionary<string, decimal>? totals = null)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text(title).Bold().FontSize(16);
                    if (!string.IsNullOrEmpty(subtitle))
                        col.Item().Text(subtitle).FontSize(10);
                    col.Item().PaddingVertical(10).LineHorizontal(1);
                });

                page.Content().Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columnsDef =>
                        {
                            foreach (var _ in columns)
                            {
                                columnsDef.RelativeColumn();
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (var column in columns)
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(column.Key).Bold();
                            }
                        });

                        foreach (var item in data)
                        {
                            foreach (var column in columns)
                            {
                                var value = column.Value(item);
                                var text = value switch
                                {
                                    decimal d => d.ToString("N2"),
                                    DateTime dt => dt.ToString("dd-MMM-yyyy"),
                                    _ => value?.ToString() ?? ""
                                };
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(text);
                            }
                        }

                        if (totals != null && totals.Any())
                        {
                            var colIndex = 0;
                            foreach (var column in columns)
                            {
                                if (totals.TryGetValue(column.Key, out var total))
                                {
                                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text(total.ToString("N2")).Bold();
                                }
                                else if (colIndex == 0)
                                {
                                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("TOTAL").Bold();
                                }
                                else
                                {
                                    table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("");
                                }
                                colIndex++;
                            }
                        }
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                    text.Span($" | Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}");
                });
            });
        });

        return document.GeneratePdf();
    }
}
