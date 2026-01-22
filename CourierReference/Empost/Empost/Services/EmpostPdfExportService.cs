using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Server.Modules.Courier.Models;
using Server.Modules.Empost.Models;

namespace Server.Modules.Empost.Services;

public interface IEmpostPdfExportService
{
    byte[] GenerateQuarterlyFeeReportPdf(EmpostQuarterlyReport report, Tenant tenant);
    byte[] GenerateAnnualSummaryReportPdf(EmpostAnnualReport report, Tenant tenant);
    byte[] GenerateSettlementStatementPdf(EmpostQuarterlyReport report, Tenant tenant, SettlementStatementData settlementData);
}

public class SettlementStatementData
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseeName { get; set; } = string.Empty;
    public DateTime LicensePeriodStart { get; set; }
    public DateTime LicensePeriodEnd { get; set; }
    public string CompanyManagerName { get; set; } = string.Empty;
    public decimal Arrears { get; set; }
    public decimal ForwardedAdvanceBalance { get; set; }
    public decimal DelayFine { get; set; }
    public decimal OtherFines { get; set; }
}

public class EmpostPdfExportService : IEmpostPdfExportService
{
    public byte[] GenerateQuarterlyFeeReportPdf(EmpostQuarterlyReport report, Tenant tenant)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => ComposeHeader(c, report, tenant));
                page.Content().Element(c => ComposeContent(c, report));
                page.Footer().Element(c => ComposeFooter(c, report));
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private void ComposeHeader(IContainer container, EmpostQuarterlyReport report, Tenant tenant)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text(tenant.Name)
                .FontSize(14)
                .Bold();

            var location = new List<string>();
            if (!string.IsNullOrEmpty(tenant.State)) location.Add(tenant.State);
            if (!string.IsNullOrEmpty(tenant.Country)) location.Add(tenant.Country);
            
            if (location.Any())
            {
                column.Item().AlignCenter().Text(string.Join(", ", location))
                    .FontSize(10);
            }

            column.Item().PaddingTop(20).AlignCenter().Text("Empost Fees Statement")
                .FontSize(14)
                .Bold();

            column.Item().PaddingTop(5).AlignCenter()
                .Text($"For the Period From {report.PeriodStart:dd MMM yyyy} to {report.PeriodEnd:dd MMM yyyy}")
                .FontSize(10);

            column.Item().PaddingTop(15);
        });
    }

    private void ComposeContent(IContainer container, EmpostQuarterlyReport report)
    {
        container.Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1.5f);
                    columns.RelativeColumn(1.5f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("Description");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Quantity");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Courier Charge");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Other Charge");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Total Charge");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("TAX");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("VAT");
                });

                var taxableItems = new List<FeeLineItem>();
                var nonTaxableItems = new List<FeeLineItem>();

                foreach (var item in report.TypeModeBreakdown.OrderBy(x => GetClassificationOrder(x.ShipmentClassification)).ThenBy(x => GetModeOrder(x.ShipmentMode)))
                {
                    var lineItem = new FeeLineItem
                    {
                        Classification = item.ShipmentClassification,
                        Mode = item.ShipmentMode,
                        Quantity = item.ShipmentCount,
                        CourierCharge = item.GrossAmount,
                        OtherCharge = 0,
                        TotalCharge = item.GrossAmount,
                        Tax = item.FeeAmount,
                        VAT = 0,
                        IsTaxable = item.TaxabilityStatus == EmpostTaxabilityStatus.Taxable
                    };

                    if (lineItem.IsTaxable)
                        taxableItems.Add(lineItem);
                    else
                        nonTaxableItems.Add(lineItem);
                }

                ShipmentClassificationType? currentClassification = null;

                foreach (var item in taxableItems)
                {
                    if (currentClassification != item.Classification)
                    {
                        currentClassification = item.Classification;
                        table.Cell().ColumnSpan(7).Element(ClassificationHeaderStyle)
                            .Text(GetClassificationDisplayName(item.Classification).ToUpper());
                    }

                    AddDataRow(table, item);
                }

                if (taxableItems.Any())
                {
                    var taxableTotal = new FeeLineItem
                    {
                        Quantity = taxableItems.Sum(x => x.Quantity),
                        CourierCharge = taxableItems.Sum(x => x.CourierCharge),
                        OtherCharge = taxableItems.Sum(x => x.OtherCharge),
                        TotalCharge = taxableItems.Sum(x => x.TotalCharge),
                        Tax = taxableItems.Sum(x => x.Tax),
                        VAT = taxableItems.Sum(x => x.VAT)
                    };
                    AddSubtotalRow(table, "Taxable", taxableTotal);
                }

                table.Cell().ColumnSpan(7).PaddingVertical(5);

                currentClassification = null;
                foreach (var item in nonTaxableItems)
                {
                    if (currentClassification != item.Classification)
                    {
                        currentClassification = item.Classification;
                        table.Cell().ColumnSpan(7).Element(ClassificationHeaderStyle)
                            .Text(GetClassificationDisplayName(item.Classification).ToUpper());
                    }

                    AddDataRow(table, item);
                }

                if (nonTaxableItems.Any())
                {
                    var nonTaxableTotal = new FeeLineItem
                    {
                        Quantity = nonTaxableItems.Sum(x => x.Quantity),
                        CourierCharge = nonTaxableItems.Sum(x => x.CourierCharge),
                        OtherCharge = nonTaxableItems.Sum(x => x.OtherCharge),
                        TotalCharge = nonTaxableItems.Sum(x => x.TotalCharge),
                        Tax = 0,
                        VAT = 0
                    };
                    AddSubtotalRow(table, "Non-Taxable", nonTaxableTotal);
                }

                var allItems = taxableItems.Concat(nonTaxableItems).ToList();
                var grandTotal = new FeeLineItem
                {
                    Quantity = allItems.Sum(x => x.Quantity),
                    CourierCharge = allItems.Sum(x => x.CourierCharge),
                    OtherCharge = allItems.Sum(x => x.OtherCharge),
                    TotalCharge = allItems.Sum(x => x.TotalCharge),
                    Tax = taxableItems.Sum(x => x.Tax),
                    VAT = taxableItems.Sum(x => x.VAT)
                };
                AddGrandTotalRow(table, grandTotal);
            });
        });
    }

    private int GetClassificationOrder(ShipmentClassificationType classification)
    {
        return classification switch
        {
            ShipmentClassificationType.Letter => 1,
            ShipmentClassificationType.Document => 2,
            ShipmentClassificationType.ParcelUpto30kg => 3,
            ShipmentClassificationType.ParcelAbove30kg => 4,
            _ => 99
        };
    }

    private int GetModeOrder(ShipmentMode mode)
    {
        return mode switch
        {
            ShipmentMode.Domestic => 1,
            ShipmentMode.Export => 2,
            ShipmentMode.Import => 3,
            ShipmentMode.Transhipment => 4,
            _ => 99
        };
    }

    private void AddDataRow(TableDescriptor table, FeeLineItem item)
    {
        var modeName = GetModeDisplayName(item.Mode);
        
        table.Cell().Element(IndentedCellStyle).Text(modeName);
        table.Cell().Element(DataCellStyle).AlignRight().Text(item.Quantity.ToString("N0"));
        table.Cell().Element(DataCellStyle).AlignRight().Text(item.CourierCharge.ToString("N2"));
        table.Cell().Element(DataCellStyle).AlignRight().Text(item.OtherCharge > 0 ? item.OtherCharge.ToString("N2") : "");
        table.Cell().Element(DataCellStyle).AlignRight().Text(item.TotalCharge.ToString("N2"));
        table.Cell().Element(DataCellStyle).AlignRight().Text(item.Tax > 0 ? item.Tax.ToString("N2") : "");
        table.Cell().Element(DataCellStyle).AlignRight().Text(item.VAT > 0 ? item.VAT.ToString("N2") : "");
    }

    private void AddSubtotalRow(TableDescriptor table, string label, FeeLineItem total)
    {
        table.Cell().ColumnSpan(7).PaddingTop(3);
        
        table.Cell().Element(SubtotalCellStyle).PaddingLeft(20).Text(label);
        table.Cell().Element(SubtotalCellStyle).AlignRight().Text(total.Quantity.ToString("N0"));
        table.Cell().Element(SubtotalCellStyle).AlignRight().Text(total.CourierCharge.ToString("N2"));
        table.Cell().Element(SubtotalCellStyle).AlignRight().Text(total.OtherCharge > 0 ? total.OtherCharge.ToString("N2") : "");
        table.Cell().Element(SubtotalCellStyle).AlignRight().Text(total.TotalCharge.ToString("N2"));
        table.Cell().Element(SubtotalCellStyle).AlignRight().Text(total.Tax > 0 ? total.Tax.ToString("N2") : "");
        table.Cell().Element(SubtotalCellStyle).AlignRight().Text(total.VAT > 0 ? total.VAT.ToString("N2") : "");
    }

    private void AddGrandTotalRow(TableDescriptor table, FeeLineItem total)
    {
        table.Cell().ColumnSpan(7).PaddingTop(10);

        table.Cell().Element(GrandTotalCellStyle).Text("Grand Total");
        table.Cell().Element(GrandTotalCellStyle).AlignRight().Text(total.Quantity.ToString("N0"));
        table.Cell().Element(GrandTotalCellStyle).AlignRight().Text(total.CourierCharge.ToString("N2"));
        table.Cell().Element(GrandTotalCellStyle).AlignRight().Text(total.OtherCharge > 0 ? total.OtherCharge.ToString("N2") : "");
        table.Cell().Element(GrandTotalCellStyle).AlignRight().Text(total.TotalCharge.ToString("N2"));
        table.Cell().Element(GrandTotalCellStyle).AlignRight().Text(total.Tax > 0 ? total.Tax.ToString("N2") : "");
        table.Cell().Element(GrandTotalCellStyle).AlignRight().Text(total.VAT > 0 ? total.VAT.ToString("N2") : "");
    }

    private IContainer HeaderCellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Black)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9).Bold());
    }

    private IContainer ClassificationHeaderStyle(IContainer container)
    {
        return container
            .PaddingTop(10)
            .PaddingBottom(3)
            .DefaultTextStyle(x => x.FontSize(9).Bold());
    }

    private IContainer DataCellStyle(IContainer container)
    {
        return container
            .PaddingVertical(3)
            .PaddingHorizontal(5)
            .DefaultTextStyle(x => x.FontSize(9));
    }

    private IContainer IndentedCellStyle(IContainer container)
    {
        return container
            .PaddingVertical(3)
            .PaddingLeft(20)
            .PaddingRight(5)
            .DefaultTextStyle(x => x.FontSize(9));
    }

    private IContainer SubtotalCellStyle(IContainer container)
    {
        return container
            .BorderTop(1)
            .BorderColor(Colors.Grey.Lighten1)
            .PaddingVertical(5)
            .PaddingHorizontal(5)
            .DefaultTextStyle(x => x.FontSize(9).Bold());
    }

    private IContainer GrandTotalCellStyle(IContainer container)
    {
        return container
            .BorderTop(2)
            .BorderColor(Colors.Black)
            .PaddingVertical(5)
            .PaddingHorizontal(5)
            .DefaultTextStyle(x => x.FontSize(10).Bold());
    }

    private void ComposeFooter(IContainer container, EmpostQuarterlyReport report)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(30).Row(row =>
            {
                row.RelativeItem().Text($"printed by System on {report.ReportGeneratedAt:dd-MM-yyyy h:mm:ss tt}")
                    .FontSize(8).FontColor(Colors.Grey.Darken1);

                row.ConstantItem(100).AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8));
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });
    }

    private string GetClassificationDisplayName(ShipmentClassificationType classification)
    {
        return classification switch
        {
            ShipmentClassificationType.Letter => "Letter",
            ShipmentClassificationType.Document => "Document",
            ShipmentClassificationType.ParcelUpto30kg => "Parcel Upto 30kg",
            ShipmentClassificationType.ParcelAbove30kg => "Parcel Above 30kg",
            _ => classification.ToString()
        };
    }

    private string GetModeDisplayName(ShipmentMode mode)
    {
        return mode switch
        {
            ShipmentMode.Domestic => "Domestic",
            ShipmentMode.Export => "Export",
            ShipmentMode.Import => "Import",
            ShipmentMode.Transhipment => "Transhipment",
            _ => mode.ToString()
        };
    }

    private class FeeLineItem
    {
        public ShipmentClassificationType Classification { get; set; }
        public ShipmentMode Mode { get; set; }
        public int Quantity { get; set; }
        public decimal CourierCharge { get; set; }
        public decimal OtherCharge { get; set; }
        public decimal TotalCharge { get; set; }
        public decimal Tax { get; set; }
        public decimal VAT { get; set; }
        public bool IsTaxable { get; set; }
    }

    public byte[] GenerateAnnualSummaryReportPdf(EmpostAnnualReport report, Tenant tenant)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(tenant.Name)
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);
                        });

                        row.ConstantItem(180).AlignRight().Column(col =>
                        {
                            col.Item().Text("EMPOST 7X COMPLIANCE")
                                .FontSize(10)
                                .Bold()
                                .FontColor(Colors.Green.Darken2);
                            col.Item().Text("Annual Summary Report")
                                .FontSize(12)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3);
                        });
                    });

                    column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                    column.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(120);
                                columns.RelativeColumn();
                            });

                            AddInfoRow(table, "License Number:", report.LicenseNumber);
                            AddInfoRow(table, "License Year:", $"{report.Year} - {report.Year + 1}");
                            AddInfoRow(table, "License Period:", 
                                $"{report.LicensePeriodStart:dd MMM yyyy} - {report.LicensePeriodEnd:dd MMM yyyy}");
                        });
                    });
                });

                page.Content().PaddingTop(15).Column(column =>
                {
                    column.Item().Text("ANNUAL FEE SUMMARY BY QUARTER")
                        .FontSize(11)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(AnnualHeaderStyle).Text("Quarter");
                            header.Cell().Element(AnnualHeaderStyle).Text("Period");
                            header.Cell().Element(AnnualHeaderStyle).AlignCenter().Text("Shipments");
                            header.Cell().Element(AnnualHeaderStyle).AlignRight().Text("Taxable Revenue");
                            header.Cell().Element(AnnualHeaderStyle).AlignRight().Text("Gross Fee");
                            header.Cell().Element(AnnualHeaderStyle).AlignRight().Text("Net Fee");
                        });

                        foreach (var quarter in report.QuarterlyReports.OrderBy(q => q.Quarter))
                        {
                            table.Cell().Element(AnnualCellStyle).Text(quarter.QuarterName);
                            table.Cell().Element(AnnualCellStyle).Text($"{quarter.PeriodStart:dd MMM} - {quarter.PeriodEnd:dd MMM yyyy}");
                            table.Cell().Element(AnnualCellStyle).AlignCenter().Text(quarter.TotalShipments.ToString("N0"));
                            table.Cell().Element(AnnualCellStyleRight).Text(quarter.TaxableGrossRevenue.ToString("N2"));
                            table.Cell().Element(AnnualCellStyleRight).Text(quarter.GrossEmpostFee.ToString("N2"));
                            table.Cell().Element(AnnualCellStyleRight).Text(quarter.NetEmpostFee.ToString("N2"));
                        }

                        table.Cell().Element(AnnualTotalStyle).Text("TOTAL").Bold();
                        table.Cell().Element(AnnualTotalStyle);
                        table.Cell().Element(AnnualTotalStyle).AlignCenter().Text(report.QuarterlyReports.Sum(q => q.TotalShipments).ToString("N0")).Bold();
                        table.Cell().Element(AnnualTotalStyleRight).Text(report.QuarterlyReports.Sum(q => q.TaxableGrossRevenue).ToString("N2")).Bold();
                        table.Cell().Element(AnnualTotalStyleRight).Text(report.QuarterlyReports.Sum(q => q.GrossEmpostFee).ToString("N2")).Bold();
                        table.Cell().Element(AnnualTotalStyleRight).Text(report.QuarterlyReports.Sum(q => q.NetEmpostFee).ToString("N2")).Bold();
                    });

                    column.Item().PaddingTop(20).Text("ANNUAL SETTLEMENT SUMMARY")
                        .FontSize(11)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                        });

                        AddAnnualSummaryRow(table, "Total Taxable Revenue", report.TaxableGrossRevenue.ToString("N2"));
                        AddAnnualSummaryRow(table, "Total Exempt Revenue", report.ExemptGrossRevenue.ToString("N2"));
                        AddAnnualSummaryRow(table, "Total Gross Revenue", report.TotalGrossRevenue.ToString("N2"));
                        AddAnnualSummaryRow(table, "Total Empost Fee (10%)", report.GrossEmpostFee.ToString("N2"));
                        AddAnnualSummaryRow(table, "Total Return Adjustments", $"({report.TotalReturnAdjustments:N2})");
                        AddAnnualSummaryRow(table, "Net Fee After Adjustments", report.NetEmpostFee.ToString("N2"));
                        AddAnnualSummaryRow(table, "Minimum Advance Amount", report.MinimumAdvanceAmount.ToString("N2"));
                        
                        var netPayable = report.NetEmpostFee - report.TotalPaid;
                        table.Cell().Element(AnnualTotalStyle).Text("Net Payable for Year").Bold();
                        table.Cell().Element(AnnualTotalStyleRight).Text((netPayable > 0 ? netPayable : 0).ToString("N2")).Bold();
                    });
                });

                page.Footer().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    column.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text($"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm}")
                            .FontSize(7)
                            .FontColor(Colors.Grey.Darken1);

                        row.ConstantItem(100).AlignRight().DefaultTextStyle(x => x.FontSize(8))
                            .Text(text =>
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });
                    });
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private void AddInfoRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
        table.Cell().Text(value).FontSize(9);
    }

    private void AddAnnualSummaryRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Element(AnnualCellStyle).Text(label);
        table.Cell().Element(AnnualCellStyleRight).Text(value);
    }

    private IContainer AnnualHeaderStyle(IContainer container)
    {
        return container
            .Background(Colors.Blue.Lighten4)
            .BorderBottom(1)
            .BorderColor(Colors.Blue.Darken2)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9).Bold());
    }

    private IContainer AnnualCellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9));
    }

    private IContainer AnnualCellStyleRight(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5)
            .AlignRight()
            .DefaultTextStyle(x => x.FontSize(9));
    }

    private IContainer AnnualTotalStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .BorderTop(2)
            .BorderColor(Colors.Blue.Darken2)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9).Bold());
    }

    private IContainer AnnualTotalStyleRight(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .BorderTop(2)
            .BorderColor(Colors.Blue.Darken2)
            .Padding(5)
            .AlignRight()
            .DefaultTextStyle(x => x.FontSize(9).Bold());
    }

    public byte[] GenerateSettlementStatementPdf(EmpostQuarterlyReport report, Tenant tenant, SettlementStatementData settlementData)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Content().Column(content =>
                {
                    content.Item().Element(c => ComposeForm9Header(c, report, settlementData));
                    content.Item().PaddingTop(10).Element(c => ComposeForm9MainTable(c, report));
                    content.Item().PaddingTop(10).Element(c => ComposeForm9FeeCalculation(c, report, settlementData));
                    content.Item().PaddingTop(10).Element(c => ComposeForm9Certifications(c, settlementData));
                    content.Item().PaddingTop(10).Element(c => ComposeForm9Footer(c));
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private void ComposeForm9Header(IContainer container, EmpostQuarterlyReport report, SettlementStatementData settlementData)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("Fees of Emirates Post Group")
                .FontSize(14).Bold();
            
            column.Item().AlignCenter().Text("Form # 9 (QUARTERLY)")
                .FontSize(12).Bold();

            column.Item().PaddingTop(5).AlignCenter()
                .Text($"for practice of licensed activities for financial period From {report.PeriodStart:dd/MM/yyyy} To {report.PeriodEnd:dd/MM/yyyy}")
                .FontSize(9);

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(text =>
                    {
                        text.Span("Licensee Name: ").Bold();
                        text.Span(settlementData.LicenseeName);
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("License No: ").Bold();
                        text.Span(settlementData.LicenseNumber);
                    });
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text(text =>
                    {
                        text.Span("License Period: From: ").Bold();
                        text.Span($"{settlementData.LicensePeriodStart:dd/MM/yyyy}");
                        text.Span(" To: ").Bold();
                        text.Span($"{settlementData.LicensePeriodEnd:dd/MM/yyyy}");
                    });
                });
            });
        });
    }

    private void ComposeForm9MainTable(IContainer container, EmpostQuarterlyReport report)
    {
        var docsLettersLocal = report.TypeModeBreakdown
            .Where(x => (x.ShipmentClassification == ShipmentClassificationType.Letter || x.ShipmentClassification == ShipmentClassificationType.Document) 
                && x.ShipmentMode == ShipmentMode.Domestic)
            .ToList();
        var docsLettersIntl = report.TypeModeBreakdown
            .Where(x => (x.ShipmentClassification == ShipmentClassificationType.Letter || x.ShipmentClassification == ShipmentClassificationType.Document) 
                && (x.ShipmentMode == ShipmentMode.Export || x.ShipmentMode == ShipmentMode.Import))
            .ToList();
        var parcelsLocal = report.TypeModeBreakdown
            .Where(x => x.ShipmentClassification == ShipmentClassificationType.ParcelUpto30kg 
                && x.ShipmentMode == ShipmentMode.Domestic)
            .ToList();
        var parcelsIntl = report.TypeModeBreakdown
            .Where(x => x.ShipmentClassification == ShipmentClassificationType.ParcelUpto30kg 
                && (x.ShipmentMode == ShipmentMode.Export || x.ShipmentMode == ShipmentMode.Import))
            .ToList();
        var unlicensedLocal = report.TypeModeBreakdown
            .Where(x => x.ShipmentClassification == ShipmentClassificationType.ParcelAbove30kg 
                && x.ShipmentMode == ShipmentMode.Domestic)
            .ToList();
        var unlicensedIntl = report.TypeModeBreakdown
            .Where(x => x.ShipmentClassification == ShipmentClassificationType.ParcelAbove30kg 
                && (x.ShipmentMode == ShipmentMode.Export || x.ShipmentMode == ShipmentMode.Import || x.ShipmentMode == ShipmentMode.Transhipment))
            .ToList();

        int docsLettersLocalCount = docsLettersLocal.Sum(x => x.ShipmentCount);
        decimal docsLettersLocalAmount = docsLettersLocal.Sum(x => x.GrossAmount);
        int docsLettersIntlCount = docsLettersIntl.Sum(x => x.ShipmentCount);
        decimal docsLettersIntlAmount = docsLettersIntl.Sum(x => x.GrossAmount);

        int parcelsLocalCount = parcelsLocal.Sum(x => x.ShipmentCount);
        decimal parcelsLocalAmount = parcelsLocal.Sum(x => x.GrossAmount);
        int parcelsIntlCount = parcelsIntl.Sum(x => x.ShipmentCount);
        decimal parcelsIntlAmount = parcelsIntl.Sum(x => x.GrossAmount);

        int unlicensedLocalCount = unlicensedLocal.Sum(x => x.ShipmentCount);
        decimal unlicensedLocalAmount = unlicensedLocal.Sum(x => x.GrossAmount);
        int unlicensedIntlCount = unlicensedIntl.Sum(x => x.ShipmentCount);
        decimal unlicensedIntlAmount = unlicensedIntl.Sum(x => x.GrossAmount);

        int totalLocalCount = docsLettersLocalCount + parcelsLocalCount;
        decimal totalLocalAmount = docsLettersLocalAmount + parcelsLocalAmount;
        int totalIntlCount = docsLettersIntlCount + parcelsIntlCount;
        decimal totalIntlAmount = docsLettersIntlAmount + parcelsIntlAmount;

        int grandTotalCount = totalLocalCount + totalIntlCount;
        decimal grandTotalAmount = totalLocalAmount + totalIntlAmount;

        int unlicensedTotalCount = unlicensedLocalCount + unlicensedIntlCount;
        decimal unlicensedTotalAmount = unlicensedLocalAmount + unlicensedIntlAmount;

        decimal totalSalesForPeriod = grandTotalAmount + unlicensedTotalAmount;

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(80);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.ConstantColumn(10);
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().RowSpan(2).Element(Form9HeaderStyle);
                header.Cell().ColumnSpan(2).Element(Form9HeaderStyle).AlignCenter().Text("Transport of documents and letters").FontSize(8).Bold();
                header.Cell().ColumnSpan(2).Element(Form9HeaderStyle).AlignCenter().Text("Transport of parcels").FontSize(8).Bold();
                header.Cell().ColumnSpan(2).Element(Form9HeaderStyle).AlignCenter().Text("Total").FontSize(8).Bold();
                header.Cell().ColumnSpan(2).Element(Form9HeaderStyle).AlignCenter().Text("Total sales of unlicensed activities\n(items and shipments exceeding 30 Kg)").FontSize(7).Bold();
                header.Cell().Element(Form9HeaderStyle);
                header.Cell().Element(Form9HeaderStyle).AlignCenter().Text("Total sales in AED\nfor period").FontSize(7).Bold();

                header.Cell().Element(Form9SubHeaderStyle).Text("Number of\nshipments/Items").FontSize(7);
                header.Cell().Element(Form9SubHeaderStyle).Text("Total sales\nin AED *").FontSize(7);
                header.Cell().Element(Form9SubHeaderStyle).Text("Number of\nshipments/Items").FontSize(7);
                header.Cell().Element(Form9SubHeaderStyle).Text("Total sales\nin AED *").FontSize(7);
                header.Cell().Element(Form9SubHeaderStyle).Text("Number of\nShipments/Items").FontSize(7);
                header.Cell().Element(Form9SubHeaderStyle).Text("Total sales\nin AED *").FontSize(7);
                header.Cell().Element(Form9SubHeaderStyle).Text("Number of\nshipments/Items").FontSize(7);
                header.Cell().Element(Form9SubHeaderStyle).Text("Total sales\nin AED *").FontSize(7);
                header.Cell().Element(Form9SubHeaderStyle);
                header.Cell().Element(Form9SubHeaderStyle);
            });

            table.Cell().Element(Form9RowStyle).Text("Local").Bold();
            table.Cell().Element(Form9CellStyle).AlignRight().Text(docsLettersLocalCount > 0 ? docsLettersLocalCount.ToString("N0") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(docsLettersLocalAmount > 0 ? docsLettersLocalAmount.ToString("N2") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(parcelsLocalCount > 0 ? parcelsLocalCount.ToString("N0") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(parcelsLocalAmount > 0 ? parcelsLocalAmount.ToString("N2") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(totalLocalCount > 0 ? totalLocalCount.ToString("N0") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(totalLocalAmount > 0 ? totalLocalAmount.ToString("N2") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(unlicensedLocalCount > 0 ? unlicensedLocalCount.ToString("N0") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(unlicensedLocalAmount > 0 ? unlicensedLocalAmount.ToString("N2") : "");
            table.Cell().Element(Form9CellStyle);
            table.Cell().Element(Form9CellStyle).AlignRight().Text((totalLocalAmount + unlicensedLocalAmount).ToString("N2"));

            table.Cell().Element(Form9RowStyle).Text("International").Bold();
            table.Cell().Element(Form9CellStyle).AlignRight().Text(docsLettersIntlCount > 0 ? docsLettersIntlCount.ToString("N0") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(docsLettersIntlAmount > 0 ? docsLettersIntlAmount.ToString("N2") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(parcelsIntlCount > 0 ? parcelsIntlCount.ToString("N0") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(parcelsIntlAmount > 0 ? parcelsIntlAmount.ToString("N2") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(totalIntlCount > 0 ? totalIntlCount.ToString("N0") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(totalIntlAmount > 0 ? totalIntlAmount.ToString("N2") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(unlicensedIntlCount > 0 ? unlicensedIntlCount.ToString("N0") : "");
            table.Cell().Element(Form9CellStyle).AlignRight().Text(unlicensedIntlAmount > 0 ? unlicensedIntlAmount.ToString("N2") : "");
            table.Cell().Element(Form9CellStyle);
            table.Cell().Element(Form9CellStyle).AlignRight().Text((totalIntlAmount + unlicensedIntlAmount).ToString("N2"));

            table.Cell().Element(Form9TotalRowStyle).Text("Total").Bold();
            table.Cell().Element(Form9TotalCellStyle).AlignRight().Text((docsLettersLocalCount + docsLettersIntlCount).ToString("N0")).Bold();
            table.Cell().Element(Form9TotalCellStyle).AlignRight().Text((docsLettersLocalAmount + docsLettersIntlAmount).ToString("N2")).Bold();
            table.Cell().Element(Form9TotalCellStyle).AlignRight().Text((parcelsLocalCount + parcelsIntlCount).ToString("N0")).Bold();
            table.Cell().Element(Form9TotalCellStyle).AlignRight().Text((parcelsLocalAmount + parcelsIntlAmount).ToString("N2")).Bold();
            table.Cell().Element(Form9TotalCellStyle).AlignRight().Text(grandTotalCount.ToString("N0")).Bold();
            table.Cell().Element(Form9TotalCellStyle).AlignRight().Text(grandTotalAmount.ToString("N2")).Bold();
            table.Cell().Element(Form9TotalCellStyle).AlignRight().Text(unlicensedTotalCount.ToString("N0")).Bold();
            table.Cell().Element(Form9TotalCellStyle).AlignRight().Text(unlicensedTotalAmount.ToString("N2")).Bold();
            table.Cell().Element(Form9TotalCellStyle);
            table.Cell().Element(Form9TotalCellStyle).AlignRight().Text(totalSalesForPeriod.ToString("N2")).Bold();
        });
    }

    private void ComposeForm9FeeCalculation(IContainer container, EmpostQuarterlyReport report, SettlementStatementData settlementData)
    {
        var taxableBreakdown = report.TypeModeBreakdown
            .Where(x => x.TaxabilityStatus == EmpostTaxabilityStatus.Taxable)
            .ToList();
        var taxableRevenue = taxableBreakdown.Sum(x => x.GrossAmount);
        var dueFee = taxableRevenue * 0.10m;
        var payableBalance = dueFee + settlementData.Arrears - settlementData.ForwardedAdvanceBalance;

        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                    });

                    table.Cell().Element(Form9FeeRowStyle).Text($"Due fees for the period (10% of {taxableRevenue:N2}):").Bold();
                    table.Cell().Element(Form9FeeValueStyle).AlignRight().Text(dueFee.ToString("N2")).Bold();

                    table.Cell().Element(Form9FeeRowStyle).Text("Arrears :");
                    table.Cell().Element(Form9FeeValueStyle).AlignRight().Text(settlementData.Arrears > 0 ? settlementData.Arrears.ToString("N2") : "");

                    table.Cell().Element(Form9FeeRowStyle);
                    table.Cell().Element(Form9FeeValueStyle);

                    table.Cell().Element(Form9FeeRowStyle).Text("Forwarded balance of advance payment:");
                    table.Cell().Element(Form9FeeValueStyle).AlignRight().Text(settlementData.ForwardedAdvanceBalance.ToString("N2"));

                    table.Cell().Element(Form9FeeRowStyle).Text("Payable balance:").Bold();
                    table.Cell().Element(Form9FeeValueStyle).AlignRight().Text(payableBalance > 0 ? payableBalance.ToString("N2") : "0.00").Bold();
                });

                col.Item().PaddingTop(5).Text("* The amount of sales must be stated before applying sales discount")
                    .FontSize(7).Italic();
            });

            row.ConstantItem(20);

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("For use of licensee").FontSize(8).Bold();
                col.Item().PaddingTop(5).Text("I hereby declare the correctness of the data stated herein:").FontSize(8);
                col.Item().PaddingTop(10).Text(text =>
                {
                    text.Span("Name of Company Manager: ").FontSize(8);
                    text.Span(settlementData.CompanyManagerName).FontSize(8).Bold();
                });
                col.Item().PaddingTop(20).Text("Signature: ________________________     Seal:").FontSize(8);
            });
        });
    }

    private void ComposeForm9Certifications(IContainer container, SettlementStatementData settlementData)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Certification of External Auditor").FontSize(9).Bold();
                col.Item().PaddingTop(5).Text("We hereby certify that data are consistent to the licensee's accounting records for the same period and licensee adheres to the Group's Laws and Regulations")
                    .FontSize(7);
                col.Item().PaddingTop(10).Text("Name of external auditor: ________________________").FontSize(8);
                col.Item().PaddingTop(10).Text("Signature: ________________________").FontSize(8);
                col.Item().PaddingTop(10).Text("Seal:").FontSize(8);
            });

            row.ConstantItem(20);

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("For use of management").FontSize(9).Bold();
                col.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(30);
                        columns.RelativeColumn(2);
                    });

                    table.Cell().Element(Form9MgmtRowStyle).Text("Due amount:");
                    table.Cell().Element(Form9MgmtRowStyle).Text("AED");
                    table.Cell().Element(Form9MgmtValueStyle);

                    table.Cell().Element(Form9MgmtRowStyle).Text("Delay fine for the period:");
                    table.Cell().Element(Form9MgmtRowStyle);
                    table.Cell().Element(Form9MgmtValueStyle).Text(settlementData.DelayFine > 0 ? settlementData.DelayFine.ToString("N2") : "");

                    table.Cell().Element(Form9MgmtRowStyle).Text("Other fines:");
                    table.Cell().Element(Form9MgmtRowStyle);
                    table.Cell().Element(Form9MgmtValueStyle).Text(settlementData.OtherFines > 0 ? settlementData.OtherFines.ToString("N2") : "");

                    table.Cell().Element(Form9MgmtTotalStyle).Text("Total").Bold();
                    table.Cell().Element(Form9MgmtTotalStyle);
                    table.Cell().Element(Form9MgmtTotalValueStyle);
                });
            });
        });
    }

    private void ComposeForm9Footer(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text("EPG/QMS/Form No. 9 (Statement Reconciliation Required Fees)/FRM/2").FontSize(7);
                row.RelativeItem().AlignRight().Text($"Date of issue: {DateTime.UtcNow:dd/MM/yyyy}").FontSize(7);
            });
        });
    }

    private IContainer Form9HeaderStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .Border(0.5f)
            .BorderColor(Colors.Black)
            .Padding(3)
            .AlignCenter()
            .DefaultTextStyle(x => x.FontSize(8).Bold());
    }

    private IContainer Form9SubHeaderStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten4)
            .Border(0.5f)
            .BorderColor(Colors.Black)
            .Padding(2)
            .AlignCenter()
            .DefaultTextStyle(x => x.FontSize(7));
    }

    private IContainer Form9RowStyle(IContainer container)
    {
        return container
            .Border(0.5f)
            .BorderColor(Colors.Black)
            .Padding(3)
            .DefaultTextStyle(x => x.FontSize(8));
    }

    private IContainer Form9CellStyle(IContainer container)
    {
        return container
            .Border(0.5f)
            .BorderColor(Colors.Black)
            .Padding(3)
            .DefaultTextStyle(x => x.FontSize(8));
    }

    private IContainer Form9TotalRowStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .Border(0.5f)
            .BorderColor(Colors.Black)
            .Padding(3)
            .DefaultTextStyle(x => x.FontSize(8).Bold());
    }

    private IContainer Form9TotalCellStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .Border(0.5f)
            .BorderColor(Colors.Black)
            .Padding(3)
            .DefaultTextStyle(x => x.FontSize(8).Bold());
    }

    private IContainer Form9FeeRowStyle(IContainer container)
    {
        return container
            .PaddingVertical(3)
            .DefaultTextStyle(x => x.FontSize(8));
    }

    private IContainer Form9FeeValueStyle(IContainer container)
    {
        return container
            .PaddingVertical(3)
            .BorderBottom(0.5f)
            .BorderColor(Colors.Black)
            .DefaultTextStyle(x => x.FontSize(8));
    }

    private IContainer Form9MgmtRowStyle(IContainer container)
    {
        return container
            .PaddingVertical(3)
            .DefaultTextStyle(x => x.FontSize(8));
    }

    private IContainer Form9MgmtValueStyle(IContainer container)
    {
        return container
            .PaddingVertical(3)
            .BorderBottom(0.5f)
            .BorderColor(Colors.Black)
            .AlignRight()
            .DefaultTextStyle(x => x.FontSize(8));
    }

    private IContainer Form9MgmtTotalStyle(IContainer container)
    {
        return container
            .PaddingVertical(3)
            .BorderTop(1)
            .BorderColor(Colors.Black)
            .DefaultTextStyle(x => x.FontSize(8).Bold());
    }

    private IContainer Form9MgmtTotalValueStyle(IContainer container)
    {
        return container
            .PaddingVertical(3)
            .BorderTop(1)
            .BorderBottom(0.5f)
            .BorderColor(Colors.Black)
            .AlignRight()
            .DefaultTextStyle(x => x.FontSize(8).Bold());
    }
}
