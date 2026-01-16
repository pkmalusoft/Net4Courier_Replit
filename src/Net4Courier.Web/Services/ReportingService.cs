using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Net4Courier.Operations.Entities;
using Net4Courier.Finance.Entities;
using Net4Courier.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Net4Courier.Web.Services;

public class ReportingService
{
    private readonly ApplicationDbContext _context;

    public ReportingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public byte[] GenerateAWBLabel(InscanMaster awb)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(4, 6, Unit.Inch);
                page.Margin(0.25f, Unit.Inch);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(col =>
                {
                    col.Item().Border(1).Padding(5).Column(inner =>
                    {
                        inner.Item().Text("Net4Courier").Bold().FontSize(14).AlignCenter();
                        inner.Item().Text("Tracking Label").FontSize(8).AlignCenter();
                        inner.Item().PaddingVertical(5).LineHorizontal(1);
                        
                        inner.Item().Text($"AWB: {awb.AWBNo}").Bold().FontSize(16).AlignCenter();
                        inner.Item().PaddingVertical(3);

                        inner.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("FROM:").Bold().FontSize(8);
                                c.Item().Text(awb.Consignor ?? "-").FontSize(9);
                                c.Item().Text(awb.ConsignorAddress1 ?? "");
                                c.Item().Text($"{awb.ConsignorCity}, {awb.ConsignorState}");
                                c.Item().Text(awb.ConsignorPhone ?? "");
                            });
                        });

                        inner.Item().PaddingVertical(5).LineHorizontal(1);

                        inner.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("TO:").Bold().FontSize(8);
                                c.Item().Text(awb.Consignee ?? "-").Bold().FontSize(11);
                                c.Item().Text(awb.ConsigneeAddress1 ?? "");
                                c.Item().Text($"{awb.ConsigneeCity}, {awb.ConsigneeState}");
                                c.Item().Text(awb.ConsigneePostalCode ?? "").Bold().FontSize(12);
                                c.Item().Text(awb.ConsigneeMobile ?? awb.ConsigneePhone ?? "");
                            });
                        });

                        inner.Item().PaddingVertical(5).LineHorizontal(1);

                        inner.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Pcs: {awb.Pieces ?? 1}").FontSize(9);
                                c.Item().Text($"Weight: {awb.Weight:N2} Kg").FontSize(9);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Date: {awb.TransactionDate:dd-MMM-yyyy}").FontSize(9);
                                c.Item().Text($"Mode: {awb.PaymentModeId}").FontSize(9);
                            });
                        });

                        if (awb.CODAmount > 0)
                        {
                            inner.Item().PaddingTop(5).Background("#FFE0E0").Padding(5).Text($"COD: {awb.CODAmount:N2}").Bold().FontSize(12).AlignCenter();
                        }

                        inner.Item().PaddingTop(5).Text(awb.CargoDescription ?? "").FontSize(8);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateInvoicePdf(Invoice invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Net4Courier").Bold().FontSize(18);
                            c.Item().Text("Courier & Logistics Services").FontSize(10);
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("INVOICE").Bold().FontSize(20);
                            c.Item().Text($"# {invoice.InvoiceNo}").FontSize(12);
                            c.Item().Text($"Date: {invoice.InvoiceDate:dd-MMM-yyyy}");
                        });
                    });
                    col.Item().PaddingVertical(10).LineHorizontal(1);
                });

                page.Content().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Bill To:").Bold();
                            c.Item().Text(invoice.CustomerName ?? "-");
                            c.Item().Text(invoice.CustomerAddress ?? "");
                            if (!string.IsNullOrEmpty(invoice.CustomerTaxNo))
                                c.Item().Text($"Tax No: {invoice.CustomerTaxNo}");
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            if (invoice.PeriodFrom.HasValue && invoice.PeriodTo.HasValue)
                            {
                                c.Item().Text($"Period: {invoice.PeriodFrom:dd-MMM-yyyy} to {invoice.PeriodTo:dd-MMM-yyyy}");
                            }
                            if (invoice.DueDate.HasValue)
                            {
                                c.Item().Text($"Due Date: {invoice.DueDate:dd-MMM-yyyy}");
                            }
                            c.Item().PaddingTop(10).Text($"Status: {invoice.Status}").Bold();
                        });
                    });

                    col.Item().PaddingVertical(15);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.ConstantColumn(40);
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(70);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background("#E0E0E0").Padding(5).Text("#").Bold();
                            header.Cell().Background("#E0E0E0").Padding(5).Text("AWB No").Bold();
                            header.Cell().Background("#E0E0E0").Padding(5).Text("Origin").Bold();
                            header.Cell().Background("#E0E0E0").Padding(5).Text("Destination").Bold();
                            header.Cell().Background("#E0E0E0").Padding(5).Text("Pcs").Bold();
                            header.Cell().Background("#E0E0E0").Padding(5).Text("Weight").Bold();
                            header.Cell().Background("#E0E0E0").Padding(5).AlignRight().Text("Amount").Bold();
                        });

                        var idx = 1;
                        foreach (var detail in invoice.Details)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(5).Text($"{idx++}");
                            table.Cell().BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(5).Text(detail.AWBNo ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(5).Text(detail.Origin ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(5).Text(detail.Destination ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(5).Text($"{detail.Pieces}");
                            table.Cell().BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(5).Text($"{detail.Weight:N2}");
                            table.Cell().BorderBottom(0.5f).BorderColor("#CCCCCC").Padding(5).AlignRight().Text($"{detail.Total:N2}");
                        }
                    });

                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            if (!string.IsNullOrEmpty(invoice.Remarks))
                            {
                                c.Item().Text("Remarks:").Bold();
                                c.Item().Text(invoice.Remarks);
                            }
                        });

                        row.ConstantItem(200).Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Sub Total:");
                                r.ConstantItem(80).AlignRight().Text($"{invoice.SubTotal:N2}");
                            });
                            if (invoice.TaxAmount > 0)
                            {
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text($"Tax ({invoice.TaxPercent}%):");
                                    r.ConstantItem(80).AlignRight().Text($"{invoice.TaxAmount:N2}");
                                });
                            }
                            if (invoice.DiscountAmount > 0)
                            {
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Discount:");
                                    r.ConstantItem(80).AlignRight().Text($"-{invoice.DiscountAmount:N2}");
                                });
                            }
                            c.Item().PaddingTop(5).LineHorizontal(1);
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Net Total:").Bold();
                                r.ConstantItem(80).AlignRight().Text($"{invoice.NetTotal:N2}").Bold();
                            });
                            if (invoice.PaidAmount > 0)
                            {
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Paid:");
                                    r.ConstantItem(80).AlignRight().Text($"{invoice.PaidAmount:N2}");
                                });
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Balance:").Bold();
                                    r.ConstantItem(80).AlignRight().Text($"{invoice.BalanceAmount:N2}").Bold();
                                });
                            }
                        });
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateReceiptPdf(Receipt receipt)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(col =>
                {
                    col.Item().AlignCenter().Text("Net4Courier").Bold().FontSize(16);
                    col.Item().AlignCenter().Text("RECEIPT").Bold().FontSize(14);
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Receipt No: {receipt.ReceiptNo}").Bold();
                        row.RelativeItem().AlignRight().Text($"Date: {receipt.ReceiptDate:dd-MMM-yyyy}");
                    });

                    col.Item().PaddingVertical(10);

                    col.Item().Text($"Received from: {receipt.CustomerName}").Bold();
                    col.Item().PaddingVertical(5);

                    col.Item().Background("#F0F0F0").Padding(10).AlignCenter().Text($"{receipt.Amount:N2}").Bold().FontSize(20);

                    col.Item().PaddingVertical(10);

                    col.Item().Text($"Payment Mode: {receipt.PaymentMode}");
                    if (!string.IsNullOrEmpty(receipt.BankName))
                        col.Item().Text($"Bank: {receipt.BankName}");
                    if (!string.IsNullOrEmpty(receipt.ChequeNo))
                    {
                        col.Item().Text($"Cheque No: {receipt.ChequeNo}");
                        if (receipt.ChequeDate.HasValue)
                            col.Item().Text($"Cheque Date: {receipt.ChequeDate:dd-MMM-yyyy}");
                    }
                    if (!string.IsNullOrEmpty(receipt.TransactionRef))
                        col.Item().Text($"Reference: {receipt.TransactionRef}");

                    if (receipt.Allocations.Any())
                    {
                        col.Item().PaddingVertical(10).LineHorizontal(0.5f);
                        col.Item().Text("Allocated to Invoices:").Bold();
                        foreach (var alloc in receipt.Allocations)
                        {
                            col.Item().Text($"  Invoice #{alloc.InvoiceId}: {alloc.AllocatedAmount:N2}");
                        }
                    }

                    if (!string.IsNullOrEmpty(receipt.Remarks))
                    {
                        col.Item().PaddingVertical(10);
                        col.Item().Text($"Remarks: {receipt.Remarks}");
                    }

                    col.Item().PaddingVertical(20);
                    col.Item().AlignRight().Text("Authorized Signature");
                    col.Item().AlignRight().PaddingTop(30).Width(150).LineHorizontal(1);
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateMAWBManifest(long mawbId)
    {
        var mawb = await _context.MasterAirwaybills
            .Include(m => m.Bags)
            .FirstOrDefaultAsync(m => m.Id == mawbId);

        if (mawb == null)
            return Array.Empty<byte>();

        var shipments = await _context.InscanMasters
            .Where(s => s.MAWBId == mawbId)
            .OrderBy(s => s.BagNo)
            .ThenBy(s => s.AWBNo)
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("MASTER AIRWAYBILL MANIFEST").Bold().FontSize(16);
                        row.ConstantItem(120).AlignRight().Text($"Status: {mawb.Status}").FontSize(10);
                    });
                    col.Item().Text($"MAWB No: {mawb.MAWBNo}").Bold().FontSize(14);
                    col.Item().PaddingVertical(10).LineHorizontal(1);
                });

                page.Content().Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn();
                            columns.ConstantColumn(80);
                            columns.RelativeColumn();
                        });

                        table.Cell().Text("Origin:").Bold();
                        table.Cell().Text($"{mawb.OriginCityName} ({mawb.OriginAirportCode})");
                        table.Cell().Text("Destination:").Bold();
                        table.Cell().Text($"{mawb.DestinationCityName} ({mawb.DestinationAirportCode})");

                        table.Cell().Text("Carrier:").Bold();
                        table.Cell().Text($"{mawb.CarrierName} ({mawb.CarrierCode})");
                        table.Cell().Text("Flight:").Bold();
                        table.Cell().Text(mawb.FlightNo ?? "-");

                        table.Cell().Text("Departure:").Bold();
                        table.Cell().Text($"{mawb.DepartureDate?.ToString("dd-MMM-yyyy")} {mawb.DepartureTime?.ToString(@"hh\:mm") ?? ""}");
                        table.Cell().Text("Arrival:").Bold();
                        table.Cell().Text($"{mawb.ArrivalDate?.ToString("dd-MMM-yyyy")} {mawb.ArrivalTime?.ToString(@"hh\:mm") ?? ""}");

                        table.Cell().Text("Total Bags:").Bold();
                        table.Cell().Text($"{mawb.TotalBags}");
                        table.Cell().Text("Total Pieces:").Bold();
                        table.Cell().Text($"{mawb.TotalPieces}");

                        table.Cell().Text("Gross Weight:").Bold();
                        table.Cell().Text($"{mawb.TotalGrossWeight:N2} Kg");
                        table.Cell().Text("Chg. Weight:").Bold();
                        table.Cell().Text($"{mawb.TotalChargeableWeight:N2} Kg");
                    });

                    col.Item().PaddingVertical(15).LineHorizontal(0.5f);

                    if (mawb.Bags?.Any() == true)
                    {
                        col.Item().Text("BAG SUMMARY").Bold().FontSize(11);
                        col.Item().PaddingVertical(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Bag No").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Seal No").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Pieces").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Gross Wt").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Chg. Wt").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Sealed").Bold();
                            });

                            foreach (var bag in mawb.Bags.OrderBy(b => b.SequenceNo))
                            {
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(bag.BagNo);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(bag.SealNo ?? "-");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(bag.PieceCount.ToString());
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text($"{bag.GrossWeight:N2}");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text($"{bag.ChargeableWeight:N2}");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(bag.IsSealed ? "Yes" : "No");
                            }
                        });
                    }

                    col.Item().PaddingVertical(15).LineHorizontal(0.5f);
                    col.Item().Text("SHIPMENT DETAILS").Bold().FontSize(11);

                    col.Item().PaddingVertical(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.ConstantColumn(90);
                            columns.RelativeColumn();
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(40);
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("#").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("AWB No").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Consignee").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Destination").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Pcs").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Wt").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Bag").Bold();
                        });

                        int seq = 0;
                        foreach (var shipment in shipments)
                        {
                            seq++;
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(seq.ToString());
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(shipment.AWBNo);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(shipment.Consignee ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(shipment.ConsigneeCity ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text((shipment.Pieces ?? 0).ToString());
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text($"{shipment.Weight ?? 0:N2}");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(shipment.BagNo ?? "-");
                        }

                        table.Cell().ColumnSpan(4).Background(Colors.Grey.Lighten4).Padding(3).Text("TOTALS").Bold();
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(3).Text(shipments.Sum(s => s.Pieces ?? 0).ToString()).Bold();
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(3).Text($"{shipments.Sum(s => s.Weight ?? 0):N2}").Bold();
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(3).Text("");
                    });

                    if (!string.IsNullOrEmpty(mawb.Remarks))
                    {
                        col.Item().PaddingTop(20).Text($"Remarks: {mawb.Remarks}");
                    }

                    col.Item().PaddingTop(40).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Prepared By:").FontSize(8);
                            c.Item().PaddingTop(20).Width(120).LineHorizontal(1);
                        });
                        row.ConstantItem(30);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Approved By:").FontSize(8);
                            c.Item().PaddingTop(20).Width(120).LineHorizontal(1);
                        });
                        row.ConstantItem(30);
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Received By:").FontSize(8);
                            c.Item().PaddingTop(20).Width(120).LineHorizontal(1);
                        });
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated: ");
                    x.Span(DateTime.Now.ToString("dd-MMM-yyyy HH:mm"));
                    x.Span(" | Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
}
