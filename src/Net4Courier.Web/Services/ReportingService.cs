using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Net4Courier.Operations.Entities;
using Net4Courier.Finance.Entities;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Kernel.Enums;
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

    public byte[] GenerateAirWaybillPdf(InscanMaster awb)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(15);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Content().Column(col =>
                {
                    col.Item().Border(1).Column(main =>
                    {
                        main.Item().Background("#1976D2").Padding(5).Row(row =>
                        {
                            row.RelativeItem().Text("Net4Courier").Bold().FontSize(14).FontColor(Colors.White);
                            row.ConstantItem(100).AlignRight().Text("AIR WAYBILL").Bold().FontSize(12).FontColor(Colors.White);
                        });

                        main.Item().Padding(8).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"AWB No: {awb.AWBNo}").Bold().FontSize(14);
                                c.Item().Text($"Date: {awb.TransactionDate:dd-MMM-yyyy}");
                            });
                            row.ConstantItem(100).AlignRight().Column(c =>
                            {
                                c.Item().Text($"Mode: {awb.PaymentModeId}").FontSize(9);
                                c.Item().Text($"Type: {awb.MovementTypeId}").FontSize(9);
                            });
                        });

                        main.Item().LineHorizontal(1);

                        main.Item().Row(row =>
                        {
                            row.RelativeItem().Border(0.5f).Padding(6).Column(c =>
                            {
                                c.Item().Text("SHIPPER / CONSIGNOR").Bold().FontSize(7).FontColor("#666666");
                                c.Item().PaddingTop(3).Text(awb.Consignor ?? "-").Bold().FontSize(10);
                                c.Item().Text(awb.ConsignorAddress1 ?? "");
                                if (!string.IsNullOrEmpty(awb.ConsignorAddress2))
                                    c.Item().Text(awb.ConsignorAddress2);
                                c.Item().Text($"{awb.ConsignorCity}, {awb.ConsignorState}");
                                c.Item().Text($"{awb.ConsignorCountry} {awb.ConsignorPostalCode}");
                                c.Item().PaddingTop(3).Text($"Tel: {awb.ConsignorPhone ?? awb.ConsignorMobile ?? "-"}");
                            });

                            row.RelativeItem().Border(0.5f).Padding(6).Column(c =>
                            {
                                c.Item().Text("CONSIGNEE / RECEIVER").Bold().FontSize(7).FontColor("#666666");
                                c.Item().PaddingTop(3).Text(awb.Consignee ?? "-").Bold().FontSize(10);
                                c.Item().Text(awb.ConsigneeAddress1 ?? "");
                                if (!string.IsNullOrEmpty(awb.ConsigneeAddress2))
                                    c.Item().Text(awb.ConsigneeAddress2);
                                c.Item().Text($"{awb.ConsigneeCity}, {awb.ConsigneeState}");
                                c.Item().Text($"{awb.ConsigneeCountry} {awb.ConsigneePostalCode}").Bold();
                                c.Item().PaddingTop(3).Text($"Tel: {awb.ConsigneeMobile ?? awb.ConsigneePhone ?? "-"}");
                            });
                        });

                        main.Item().Border(0.5f).Padding(6).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Text("Pieces").Bold().FontSize(7).FontColor("#666666");
                            table.Cell().Text("Weight (Kg)").Bold().FontSize(7).FontColor("#666666");
                            table.Cell().Text("Vol. Weight").Bold().FontSize(7).FontColor("#666666");
                            table.Cell().Text("Chg. Weight").Bold().FontSize(7).FontColor("#666666");

                            table.Cell().Text($"{awb.Pieces ?? 1}").Bold().FontSize(11);
                            table.Cell().Text($"{awb.Weight ?? 0:N2}").Bold().FontSize(11);
                            table.Cell().Text($"{awb.VolumetricWeight ?? 0:N2}").FontSize(11);
                            table.Cell().Text($"{awb.ChargeableWeight ?? awb.Weight ?? 0:N2}").Bold().FontSize(11);
                        });

                        main.Item().Border(0.5f).Padding(6).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("DIMENSIONS (cm)").Bold().FontSize(7).FontColor("#666666");
                                c.Item().Text($"L: {awb.Length ?? 0} x W: {awb.Width ?? 0} x H: {awb.Height ?? 0}");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("CBM").Bold().FontSize(7).FontColor("#666666");
                                c.Item().Text($"{awb.CBM ?? 0:N4}");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("DOCUMENT TYPE").Bold().FontSize(7).FontColor("#666666");
                                c.Item().Text($"{awb.DocumentTypeId}");
                            });
                        });

                        main.Item().Border(0.5f).Padding(6).Column(c =>
                        {
                            c.Item().Text("CARGO DESCRIPTION").Bold().FontSize(7).FontColor("#666666");
                            c.Item().Text(awb.CargoDescription ?? "-");
                        });

                        if (!string.IsNullOrEmpty(awb.SpecialInstructions) || !string.IsNullOrEmpty(awb.HandlingInstructions))
                        {
                            main.Item().Border(0.5f).Padding(6).Column(c =>
                            {
                                c.Item().Text("SPECIAL INSTRUCTIONS").Bold().FontSize(7).FontColor("#666666");
                                if (!string.IsNullOrEmpty(awb.SpecialInstructions))
                                    c.Item().Text(awb.SpecialInstructions);
                                if (!string.IsNullOrEmpty(awb.HandlingInstructions))
                                    c.Item().Text($"Handling: {awb.HandlingInstructions}");
                            });
                        }

                        main.Item().Border(0.5f).Padding(6).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                            });

                            table.Cell().Text("Freight Charge:").FontSize(8);
                            table.Cell().AlignRight().Text($"{awb.CourierCharge ?? 0:N2}").FontSize(8);

                            if ((awb.FuelSurcharge ?? 0) > 0)
                            {
                                table.Cell().Text("Fuel Surcharge:").FontSize(8);
                                table.Cell().AlignRight().Text($"{awb.FuelSurcharge:N2}").FontSize(8);
                            }

                            if ((awb.OtherCharge ?? 0) > 0)
                            {
                                table.Cell().Text("Other Charges:").FontSize(8);
                                table.Cell().AlignRight().Text($"{awb.OtherCharge:N2}").FontSize(8);
                            }

                            if ((awb.TaxAmount ?? 0) > 0)
                            {
                                table.Cell().Text($"Tax ({awb.TaxPercent ?? 0}%):").FontSize(8);
                                table.Cell().AlignRight().Text($"{awb.TaxAmount:N2}").FontSize(8);
                            }

                            table.Cell().PaddingTop(3).Text("NET TOTAL:").Bold().FontSize(9);
                            table.Cell().PaddingTop(3).AlignRight().Text($"{awb.NetTotal ?? 0:N2}").Bold().FontSize(10);
                        });

                        if (awb.IsCOD && (awb.CODAmount ?? 0) > 0)
                        {
                            main.Item().Background("#FFEBEE").Padding(6).Row(row =>
                            {
                                row.RelativeItem().Text("CASH ON DELIVERY (COD)").Bold().FontSize(9);
                                row.ConstantItem(100).AlignRight().Text($"{awb.CODAmount:N2}").Bold().FontSize(12);
                            });
                        }

                        main.Item().Padding(6).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Shipper's Signature").FontSize(7);
                                c.Item().PaddingTop(20).Width(100).LineHorizontal(0.5f);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Receiver's Signature").FontSize(7);
                                c.Item().PaddingTop(20).Width(100).LineHorizontal(0.5f);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Date & Time").FontSize(7);
                                c.Item().PaddingTop(20).Width(100).LineHorizontal(0.5f);
                            });
                        });

                        main.Item().Background("#F5F5F5").Padding(4).Text(x =>
                        {
                            x.Span("Terms: ").FontSize(6);
                            x.Span("Goods carried subject to standard terms and conditions. Max liability limited per kg. Insurance recommended.").FontSize(6);
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateManifestLabel(InscanMaster awb, string? bagNo = null, string? mawbNo = null)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(4, 6, Unit.Inch);
                page.Margin(0.15f, Unit.Inch);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Content().Border(2).Column(col =>
                {
                    col.Item().Background("#1976D2").Padding(6).Row(row =>
                    {
                        row.RelativeItem().Text("Net4Courier").Bold().FontSize(12).FontColor(Colors.White);
                        row.ConstantItem(80).AlignRight().Text("MANIFEST LABEL").FontSize(8).FontColor(Colors.White);
                    });

                    col.Item().Padding(8).Column(inner =>
                    {
                        inner.Item().AlignCenter().Text(awb.AWBNo).Bold().FontSize(18);

                        inner.Item().PaddingVertical(5).LineHorizontal(1);

                        if (!string.IsNullOrEmpty(bagNo))
                        {
                            inner.Item().Background("#E3F2FD").Padding(4).Row(row =>
                            {
                                row.RelativeItem().Text("BAG:").Bold();
                                row.ConstantItem(120).AlignRight().Text(bagNo).Bold().FontSize(12);
                            });
                        }

                        if (!string.IsNullOrEmpty(mawbNo))
                        {
                            inner.Item().Background("#FFF3E0").Padding(4).Row(row =>
                            {
                                row.RelativeItem().Text("MAWB:").Bold();
                                row.ConstantItem(120).AlignRight().Text(mawbNo).Bold().FontSize(10);
                            });
                        }

                        inner.Item().PaddingTop(8).Text("TO:").Bold().FontSize(8).FontColor("#666666");
                        inner.Item().Text(awb.Consignee ?? "-").Bold().FontSize(12);
                        inner.Item().Text(awb.ConsigneeAddress1 ?? "");
                        inner.Item().Text($"{awb.ConsigneeCity}, {awb.ConsigneeState}").Bold();
                        inner.Item().Text(awb.ConsigneePostalCode ?? "").Bold().FontSize(14);
                        inner.Item().Text(awb.ConsigneeCountry ?? "");

                        inner.Item().PaddingTop(8).LineHorizontal(0.5f);

                        inner.Item().PaddingTop(5).Text("FROM:").Bold().FontSize(8).FontColor("#666666");
                        inner.Item().Text(awb.Consignor ?? "-").FontSize(10);
                        inner.Item().Text($"{awb.ConsignorCity}, {awb.ConsignorCountry}");

                        inner.Item().PaddingTop(8).LineHorizontal(0.5f);

                        inner.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Pcs").FontSize(8).FontColor("#666666");
                                c.Item().Text($"{awb.Pieces ?? 1}").Bold().FontSize(14);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Weight").FontSize(8).FontColor("#666666");
                                c.Item().Text($"{awb.Weight ?? 0:N1} Kg").Bold().FontSize(14);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Date").FontSize(8).FontColor("#666666");
                                c.Item().Text(awb.TransactionDate.ToString("dd-MMM")).Bold().FontSize(11);
                            });
                        });

                        if (awb.IsCOD && (awb.CODAmount ?? 0) > 0)
                        {
                            inner.Item().PaddingTop(8).Background("#FFCDD2").Padding(6).AlignCenter().Text($"COD: {awb.CODAmount:N2}").Bold().FontSize(14);
                        }

                        inner.Item().PaddingTop(8).Text(awb.CargoDescription ?? "").FontSize(8);
                    });

                    col.Item().Background("#EEEEEE").Padding(4).AlignCenter().Text($"Movement: {awb.MovementTypeId}").FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateExportManifest(long mawbId)
    {
        var mawb = await _context.MasterAirwaybills
            .Include(m => m.Bags)
            .FirstOrDefaultAsync(m => m.Id == mawbId);

        if (mawb == null)
            return Array.Empty<byte>();

        var shipments = await _context.InscanMasters
            .Where(s => s.MAWBId == mawbId && s.MovementTypeId != MovementType.Domestic)
            .OrderBy(s => s.ConsigneeCountry)
            .ThenBy(s => s.ConsigneeCity)
            .ThenBy(s => s.AWBNo)
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("EXPORT MANIFEST").Bold().FontSize(18);
                            c.Item().Text("Net4Courier - International Shipments").FontSize(10);
                        });
                        row.ConstantItem(200).AlignRight().Column(c =>
                        {
                            c.Item().Text($"MAWB: {mawb.MAWBNo}").Bold().FontSize(12);
                            c.Item().Text($"Date: {DateTime.Now:dd-MMM-yyyy}");
                            c.Item().Text($"Status: {mawb.Status}").FontSize(10);
                        });
                    });

                    col.Item().PaddingVertical(8).LineHorizontal(2);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn();
                            columns.ConstantColumn(80);
                            columns.RelativeColumn();
                            columns.ConstantColumn(80);
                            columns.RelativeColumn();
                        });

                        table.Cell().Text("Origin:").Bold();
                        table.Cell().Text($"{mawb.OriginCityName}, {mawb.OriginCountryName} ({mawb.OriginAirportCode})");
                        table.Cell().Text("Destination:").Bold();
                        table.Cell().Text($"{mawb.DestinationCityName}, {mawb.DestinationCountryName} ({mawb.DestinationAirportCode})");
                        table.Cell().Text("Carrier:").Bold();
                        table.Cell().Text($"{mawb.CarrierName} - {mawb.FlightNo}");

                        table.Cell().Text("Departure:").Bold();
                        table.Cell().Text($"{mawb.DepartureDate?.ToString("dd-MMM-yyyy")} {mawb.DepartureTime?.ToString(@"hh\:mm")}");
                        table.Cell().Text("Arrival:").Bold();
                        table.Cell().Text($"{mawb.ArrivalDate?.ToString("dd-MMM-yyyy")} {mawb.ArrivalTime?.ToString(@"hh\:mm")}");
                        table.Cell().Text("Total Bags:").Bold();
                        table.Cell().Text($"{mawb.TotalBags}");
                    });

                    col.Item().PaddingTop(10);
                });

                page.Content().Column(col =>
                {
                    var groupedByCountry = shipments.GroupBy(s => s.ConsigneeCountry ?? "Unknown");

                    foreach (var countryGroup in groupedByCountry)
                    {
                        col.Item().Background("#E3F2FD").Padding(5).Text($"DESTINATION COUNTRY: {countryGroup.Key}").Bold().FontSize(10);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);
                                columns.ConstantColumn(90);
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(35);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(60);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("#").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("AWB No").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Consignee").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("City").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Pcs").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Weight").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Value").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Contents").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Bag").Bold();
                            });

                            int seq = 0;
                            foreach (var shipment in countryGroup)
                            {
                                seq++;
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(seq.ToString());
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(shipment.AWBNo);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(shipment.Consignee ?? "-");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(shipment.ConsigneeCity ?? "-");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text((shipment.Pieces ?? 0).ToString());
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text($"{shipment.Weight ?? 0:N2}");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text($"{shipment.CustomsValue ?? 0:N2}");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text((shipment.CargoDescription ?? "-").Length > 15 ? (shipment.CargoDescription ?? "").Substring(0, 15) + "..." : shipment.CargoDescription ?? "-");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(shipment.BagNo ?? "-");
                            }
                        });

                        col.Item().PaddingBottom(5).Row(row =>
                        {
                            row.RelativeItem();
                            row.ConstantItem(200).Background(Colors.Grey.Lighten4).Padding(3).Text($"Country Total: {countryGroup.Count()} shipments, {countryGroup.Sum(s => s.Pieces ?? 0)} pcs, {countryGroup.Sum(s => s.Weight ?? 0):N2} kg").Bold().FontSize(8);
                        });

                        col.Item().PaddingBottom(10);
                    }

                    col.Item().PaddingTop(10).Background("#1976D2").Padding(8).Row(row =>
                    {
                        row.RelativeItem().Text($"GRAND TOTAL: {shipments.Count} Shipments").Bold().FontColor(Colors.White);
                        row.ConstantItem(100).Text($"{shipments.Sum(s => s.Pieces ?? 0)} Pieces").Bold().FontColor(Colors.White);
                        row.ConstantItem(120).Text($"{shipments.Sum(s => s.Weight ?? 0):N2} Kg").Bold().FontColor(Colors.White);
                        row.ConstantItem(120).Text($"Value: {shipments.Sum(s => s.CustomsValue ?? 0):N2}").Bold().FontColor(Colors.White);
                    });

                    col.Item().PaddingTop(30).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Prepared By:").FontSize(8);
                            c.Item().PaddingTop(25).Width(150).LineHorizontal(0.5f);
                            c.Item().Text("Name / Signature / Date").FontSize(7);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Customs Officer:").FontSize(8);
                            c.Item().PaddingTop(25).Width(150).LineHorizontal(0.5f);
                            c.Item().Text("Stamp / Signature").FontSize(7);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Carrier Representative:").FontSize(8);
                            c.Item().PaddingTop(25).Width(150).LineHorizontal(0.5f);
                            c.Item().Text("Name / Signature").FontSize(7);
                        });
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Export Manifest - Generated: ");
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

    public async Task<byte[]> GenerateDomesticManifest(long mawbId)
    {
        var mawb = await _context.MasterAirwaybills
            .Include(m => m.Bags)
            .FirstOrDefaultAsync(m => m.Id == mawbId);

        if (mawb == null)
            return Array.Empty<byte>();

        var shipments = await _context.InscanMasters
            .Where(s => s.MAWBId == mawbId && s.MovementTypeId == MovementType.Domestic)
            .OrderBy(s => s.ConsigneeState)
            .ThenBy(s => s.ConsigneeCity)
            .ThenBy(s => s.AWBNo)
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("DOMESTIC MANIFEST").Bold().FontSize(16);
                            c.Item().Text("Net4Courier - Local Shipments").FontSize(10);
                        });
                        row.ConstantItem(150).AlignRight().Column(c =>
                        {
                            c.Item().Text($"MAWB: {mawb.MAWBNo}").Bold().FontSize(11);
                            c.Item().Text($"Date: {DateTime.Now:dd-MMM-yyyy}");
                        });
                    });

                    col.Item().PaddingVertical(8).LineHorizontal(2);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Origin: {mawb.OriginCityName}");
                            c.Item().Text($"Destination: {mawb.DestinationCityName}");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Carrier: {mawb.CarrierName} - {mawb.FlightNo}");
                            c.Item().Text($"Departure: {mawb.DepartureDate?.ToString("dd-MMM-yyyy")} {mawb.DepartureTime?.ToString(@"hh\:mm")}");
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text($"Total Bags: {mawb.TotalBags}");
                            c.Item().Text($"Shipments: {shipments.Count}");
                        });
                    });

                    col.Item().PaddingTop(10);
                });

                page.Content().Column(col =>
                {
                    var groupedByRegion = shipments.GroupBy(s => s.ConsigneeState ?? "Unknown");

                    foreach (var regionGroup in groupedByRegion)
                    {
                        col.Item().Background("#E8F5E9").Padding(5).Text($"REGION: {regionGroup.Key}").Bold().FontSize(10);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);
                                columns.ConstantColumn(90);
                                columns.RelativeColumn();
                                columns.ConstantColumn(90);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(35);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(60);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("#").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("AWB No").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Consignee").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("City").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Postal").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Pcs").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Weight").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Bag").Bold();
                            });

                            int seq = 0;
                            foreach (var shipment in regionGroup)
                            {
                                seq++;
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(seq.ToString());
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(shipment.AWBNo);
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(shipment.Consignee ?? "-");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(shipment.ConsigneeCity ?? "-");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(shipment.ConsigneePostalCode ?? "-");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text((shipment.Pieces ?? 0).ToString());
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text($"{shipment.Weight ?? 0:N2}");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(shipment.BagNo ?? "-");
                            }
                        });

                        col.Item().PaddingBottom(5).Row(row =>
                        {
                            row.RelativeItem();
                            row.ConstantItem(180).Background(Colors.Grey.Lighten4).Padding(3).Text($"Region Total: {regionGroup.Count()} shipments, {regionGroup.Sum(s => s.Pieces ?? 0)} pcs, {regionGroup.Sum(s => s.Weight ?? 0):N2} kg").Bold().FontSize(8);
                        });

                        col.Item().PaddingBottom(10);
                    }

                    col.Item().PaddingTop(10).Background("#4CAF50").Padding(8).Row(row =>
                    {
                        row.RelativeItem().Text($"GRAND TOTAL: {shipments.Count} Shipments").Bold().FontColor(Colors.White);
                        row.ConstantItem(100).Text($"{shipments.Sum(s => s.Pieces ?? 0)} Pieces").Bold().FontColor(Colors.White);
                        row.ConstantItem(100).Text($"{shipments.Sum(s => s.Weight ?? 0):N2} Kg").Bold().FontColor(Colors.White);
                    });

                    col.Item().PaddingTop(30).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Prepared By:").FontSize(8);
                            c.Item().PaddingTop(25).Width(130).LineHorizontal(0.5f);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Dispatched By:").FontSize(8);
                            c.Item().PaddingTop(25).Width(130).LineHorizontal(0.5f);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Received By:").FontSize(8);
                            c.Item().PaddingTop(25).Width(130).LineHorizontal(0.5f);
                        });
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Domestic Manifest - Generated: ");
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

    public async Task<byte[]> GenerateManifestLabels(long mawbId)
    {
        var mawb = await _context.MasterAirwaybills.FindAsync(mawbId);
        if (mawb == null)
            return Array.Empty<byte>();

        var shipments = await _context.InscanMasters
            .Where(s => s.MAWBId == mawbId)
            .OrderBy(s => s.BagNo)
            .ThenBy(s => s.AWBNo)
            .ToListAsync();

        if (!shipments.Any())
            return Array.Empty<byte>();

        var document = Document.Create(container =>
        {
            foreach (var shipment in shipments)
            {
                container.Page(page =>
                {
                    page.Size(4, 6, Unit.Inch);
                    page.Margin(0.15f, Unit.Inch);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Content().Border(2).Column(col =>
                    {
                        col.Item().Background("#1976D2").Padding(6).Row(row =>
                        {
                            row.RelativeItem().Text("Net4Courier").Bold().FontSize(12).FontColor(Colors.White);
                            row.ConstantItem(80).AlignRight().Text("MANIFEST LABEL").FontSize(8).FontColor(Colors.White);
                        });

                        col.Item().Padding(8).Column(inner =>
                        {
                            inner.Item().AlignCenter().Text(shipment.AWBNo).Bold().FontSize(18);

                            inner.Item().PaddingVertical(5).LineHorizontal(1);

                            if (!string.IsNullOrEmpty(shipment.BagNo))
                            {
                                inner.Item().Background("#E3F2FD").Padding(4).Row(row =>
                                {
                                    row.RelativeItem().Text("BAG:").Bold();
                                    row.ConstantItem(120).AlignRight().Text(shipment.BagNo).Bold().FontSize(12);
                                });
                            }

                            if (!string.IsNullOrEmpty(mawb.MAWBNo))
                            {
                                inner.Item().Background("#FFF3E0").Padding(4).Row(row =>
                                {
                                    row.RelativeItem().Text("MAWB:").Bold();
                                    row.ConstantItem(120).AlignRight().Text(mawb.MAWBNo).Bold().FontSize(10);
                                });
                            }

                            inner.Item().PaddingTop(8).Text("TO:").Bold().FontSize(8).FontColor("#666666");
                            inner.Item().Text(shipment.Consignee ?? "-").Bold().FontSize(12);
                            inner.Item().Text(shipment.ConsigneeAddress1 ?? "");
                            inner.Item().Text($"{shipment.ConsigneeCity}, {shipment.ConsigneeState}").Bold();
                            inner.Item().Text(shipment.ConsigneePostalCode ?? "").Bold().FontSize(14);
                            inner.Item().Text(shipment.ConsigneeCountry ?? "");

                            inner.Item().PaddingTop(8).LineHorizontal(0.5f);

                            inner.Item().PaddingTop(5).Text("FROM:").Bold().FontSize(8).FontColor("#666666");
                            inner.Item().Text(shipment.Consignor ?? "-").FontSize(10);
                            inner.Item().Text($"{shipment.ConsignorCity}, {shipment.ConsignorCountry}");

                            inner.Item().PaddingTop(8).LineHorizontal(0.5f);

                            inner.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Pcs").FontSize(8).FontColor("#666666");
                                    c.Item().Text($"{shipment.Pieces ?? 1}").Bold().FontSize(14);
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Weight").FontSize(8).FontColor("#666666");
                                    c.Item().Text($"{shipment.Weight ?? 0:N1} Kg").Bold().FontSize(14);
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Date").FontSize(8).FontColor("#666666");
                                    c.Item().Text(shipment.TransactionDate.ToString("dd-MMM")).Bold().FontSize(11);
                                });
                            });

                            if (shipment.IsCOD && (shipment.CODAmount ?? 0) > 0)
                            {
                                inner.Item().PaddingTop(8).Background("#FFCDD2").Padding(6).AlignCenter().Text($"COD: {shipment.CODAmount:N2}").Bold().FontSize(14);
                            }

                            inner.Item().PaddingTop(8).Text(shipment.CargoDescription ?? "").FontSize(8);
                        });

                        col.Item().Background("#EEEEEE").Padding(4).AlignCenter().Text($"Movement: {shipment.MovementTypeId}").FontSize(8);
                    });
                });
            }
        });

        return document.GeneratePdf();
    }
}
