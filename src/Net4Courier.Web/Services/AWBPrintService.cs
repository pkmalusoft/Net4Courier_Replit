using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Net4Courier.Operations.Entities;
using Net4Courier.Kernel.Enums;
using Microsoft.AspNetCore.Hosting;

namespace Net4Courier.Web.Services;

public class AWBPrintService
{
    private byte[]? _logoData;

    public AWBPrintService()
    {
    }

    public void SetLogoData(byte[]? logoData)
    {
        _logoData = logoData;
    }

    public byte[] GenerateA5AWB(InscanMaster shipment, string? companyName = null, byte[]? logoData = null, string? website = null, string? branchCurrency = null)
    {
        var effectiveLogo = logoData ?? _logoData;
        var currency = branchCurrency ?? shipment.Currency ?? "AED";
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.MarginHorizontal(16);
                page.MarginVertical(12);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Content().Column(column =>
                {
                    column.Spacing(0);
                    A5Header(column, shipment, companyName ?? "Net4Courier", effectiveLogo);
                    A5InfoRow(column, shipment);
                    A5MiddleSection(column, shipment, currency);
                    A5PodFooter(column, shipment, website);
                });
            });
        });

        return document.GeneratePdf();
    }

    private void A5Header(ColumnDescriptor column, InscanMaster shipment, string companyName, byte[]? logoData)
    {
        var effectiveLogo = logoData ?? _logoData;
        var serviceDesc = GetMovementTypeDisplay(shipment.MovementTypeId);
        var originCity = shipment.ConsignorCity ?? GetCountryDisplayCode(shipment.OriginPortCode, shipment.ConsignorCountry);
        var destCity = shipment.ConsigneeCity ?? GetCountryDisplayCode(shipment.DestinationPortCode, shipment.ConsigneeCountry);

        column.Item().BorderBottom(2).PaddingBottom(5).Row(row =>
        {
            row.RelativeItem(3).Column(left =>
            {
                if (effectiveLogo != null)
                {
                    left.Item().Height(22).Image(effectiveLogo).FitHeight();
                }
                else
                {
                    left.Item().Text(companyName).Bold().FontSize(18).FontColor("#1e3a5f");
                }
                left.Item().PaddingTop(2).Row(tagRow =>
                {
                    tagRow.AutoItem().Background("#1e3a5f").PaddingHorizontal(6).PaddingVertical(2)
                        .Text(serviceDesc.ToUpper()).FontColor(Colors.White).Bold().FontSize(7);
                });
            });

            row.RelativeItem(4).AlignCenter().AlignMiddle().Row(routeRow =>
            {
                routeRow.RelativeItem().AlignCenter().Column(rc =>
                {
                    rc.Item().AlignCenter().Border(1.5f).PaddingHorizontal(16).PaddingVertical(4).Row(pill =>
                    {
                        pill.AutoItem().Text(originCity.ToUpper()).Bold().FontSize(12);
                        pill.AutoItem().PaddingHorizontal(8).Text("\u2192").Bold().FontSize(14);
                        pill.AutoItem().Text(destCity.ToUpper()).Bold().FontSize(12).Underline();
                    });
                });
            });

            row.RelativeItem(3).AlignRight().Column(right =>
            {
                right.Item().AlignRight().Text("WAYBILL NUMBER").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                right.Item().AlignRight().Text(shipment.AWBNo ?? "").Bold().FontSize(17).LetterSpacing(0.1f);
            });
        });
    }

    private void A5InfoRow(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().PaddingTop(3).PaddingBottom(3).Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("Account No  ").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(shipment.CustomerId?.ToString() ?? "").Bold().FontSize(9);
            });
            row.RelativeItem().AlignCenter().Text(text =>
            {
                text.Span("Booking Date : ").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(shipment.TransactionDate.ToString("dd/MM/yyyy")).Bold().FontSize(9);
            });
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Shipper's Reference  ").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(shipment.ReferenceNo ?? "").FontSize(9);
            });
        });
    }

    private void A5MiddleSection(ColumnDescriptor column, InscanMaster shipment, string currency = "AED")
    {
        column.Item().PaddingTop(3).Row(mainRow =>
        {
            mainRow.RelativeItem(6).PaddingRight(8).Column(leftCol =>
            {
                leftCol.Item().Row(cardsRow =>
                {
                    cardsRow.RelativeItem().Border(1).Column(fromCard =>
                    {
                        A5ShipperCard(fromCard, shipment);
                    });
                    cardsRow.RelativeItem().Border(2).Column(toCard =>
                    {
                        A5ReceiverCard(toCard, shipment);
                    });
                });

                var itemDescParts = new List<string>();
                if (!string.IsNullOrWhiteSpace(shipment.CargoDescription))
                    itemDescParts.Add(shipment.CargoDescription);
                if (!string.IsNullOrWhiteSpace(shipment.Remarks))
                    itemDescParts.Add(shipment.Remarks);
                var itemDesc = string.Join(" | ", itemDescParts);
                leftCol.Item().PaddingTop(3).Border(1).Background("#fffbeb").Padding(3).Row(instrRow =>
                {
                    instrRow.ConstantItem(140).Text("Item Description / Special Instruction:").Bold().FontSize(8);
                    instrRow.RelativeItem().Text(itemDesc).FontSize(8).Italic();
                });
            });

            mainRow.RelativeItem(4).Column(rightCol =>
            {
                A5BarcodeBox(rightCol, shipment);
                A5WeightPiecesBox(rightCol, shipment);
                A5CollectCashBox(rightCol, shipment, currency);
            });
        });
    }

    private void A5ShipperCard(ColumnDescriptor card, InscanMaster shipment)
    {
        var address = $"{shipment.ConsignorAddress1} {shipment.ConsignorAddress2}".Trim();
        if (!string.IsNullOrEmpty(shipment.ConsignorCity))
            address += $", {shipment.ConsignorCity}";
        if (!string.IsNullOrEmpty(shipment.ConsignorCountry))
            address += $", {shipment.ConsignorCountry}";

        card.Item().Background("#e2e8f0").BorderBottom(1).PaddingHorizontal(6).PaddingVertical(2)
            .Text("FROM (SHIPPER)").Bold().FontSize(8);
        card.Item().Padding(5).Column(content =>
        {
            content.Item().Text(shipment.Consignor ?? "").Bold().FontSize(10);
            content.Item().PaddingTop(1).Text(address).FontSize(8).FontColor(Colors.Grey.Darken2);
            content.Item().PaddingTop(3).Text(text =>
            {
                text.Span("Tel: ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(shipment.ConsignorPhone ?? "").Bold().FontSize(9);
                if (!string.IsNullOrWhiteSpace(shipment.ConsignorMobile))
                {
                    text.Span("   Mob: ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    text.Span(shipment.ConsignorMobile).Bold().FontSize(9);
                }
            });
        });
    }

    private void A5ReceiverCard(ColumnDescriptor card, InscanMaster shipment)
    {
        var address = $"{shipment.ConsigneeAddress1} {shipment.ConsigneeAddress2}".Trim();
        if (!string.IsNullOrEmpty(shipment.ConsigneeCity))
            address += $", {shipment.ConsigneeCity}";
        if (!string.IsNullOrEmpty(shipment.ConsigneeCountry))
            address += $", {shipment.ConsigneeCountry}";

        card.Item().Background(Colors.Black).PaddingHorizontal(6).PaddingVertical(2)
            .Text("TO (RECEIVER)").FontColor(Colors.White).Bold().FontSize(8);
        card.Item().Padding(5).Column(content =>
        {
            content.Item().Text(shipment.Consignee ?? "").Bold().FontSize(11);
            content.Item().PaddingTop(1).Text(address).FontSize(8);
            content.Item().PaddingTop(3).Text(text =>
            {
                text.Span("Tel: ").FontSize(8).FontColor(Colors.Grey.Darken1);
                text.Span(shipment.ConsigneePhone ?? "").Bold().FontSize(9);
                if (!string.IsNullOrWhiteSpace(shipment.ConsigneeMobile))
                {
                    text.Span("   Mob: ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    text.Span(shipment.ConsigneeMobile).Bold().FontSize(9);
                }
            });
        });
    }

    private void A5BarcodeBox(ColumnDescriptor col, InscanMaster shipment)
    {
        col.Item().Border(1).Padding(5).AlignCenter().Column(bc =>
        {
            if (shipment.BarcodeImage != null)
            {
                bc.Item().AlignCenter().Height(32).Image(shipment.BarcodeImage);
            }
            bc.Item().PaddingTop(3).AlignCenter().Text(shipment.AWBNo ?? "").Bold().FontSize(10).LetterSpacing(0.4f);
        });
    }

    private void A5WeightPiecesBox(ColumnDescriptor col, InscanMaster shipment)
    {
        var weight = shipment.ChargeableWeight ?? shipment.Weight ?? 0;
        var vWeight = shipment.VolumetricWeight ?? 0;
        var pieces = shipment.Pieces ?? 1;

        col.Item().PaddingTop(3).Border(1).Row(row =>
        {
            row.RelativeItem().BorderRight(1).PaddingVertical(3).AlignCenter().Column(c =>
            {
                c.Item().AlignCenter().Text("PIECES").Bold().FontSize(7).FontColor(Colors.Grey.Darken1);
                c.Item().AlignCenter().Text(pieces.ToString()).Bold().FontSize(14);
            });
            row.RelativeItem().BorderRight(1).PaddingVertical(3).AlignCenter().Column(c =>
            {
                c.Item().AlignCenter().Text("WEIGHT").Bold().FontSize(7).FontColor(Colors.Grey.Darken1);
                c.Item().AlignCenter().Text($"{weight:F2} KG").Bold().FontSize(14);
            });
            row.RelativeItem().PaddingVertical(3).AlignCenter().Column(c =>
            {
                c.Item().AlignCenter().Text("V.WEIGHT").Bold().FontSize(7).FontColor(Colors.Grey.Darken1);
                c.Item().AlignCenter().Text($"{vWeight:F2}").Bold().FontSize(14);
            });
        });

        if (shipment.IsReturnedToConsignor)
        {
            col.Item().PaddingTop(2).AlignCenter().Background("#dc2626").PaddingHorizontal(6).PaddingVertical(1)
                .Text("RETURN SERVICE").FontColor(Colors.White).Bold().FontSize(7);
        }
    }

    private void A5CollectCashBox(ColumnDescriptor col, InscanMaster shipment, string currency = "AED")
    {
        var codAmount = shipment.IsCOD ? (shipment.CODAmount ?? 0) : 0;
        var dutyAmount = shipment.DutyVatAmount ?? 0;
        var totalAmount = codAmount + dutyAmount;

        col.Item().PaddingTop(3).Border(1).Column(cash =>
        {
            cash.Item().Background(Colors.Black).PaddingHorizontal(8).PaddingVertical(4).Row(headerRow =>
            {
                headerRow.AutoItem().AlignMiddle().Text("\u2709").FontColor(Colors.White).FontSize(10);
                headerRow.AutoItem().PaddingLeft(6).AlignMiddle().Text("COLLECT PAYMENT").FontColor(Colors.White).Bold().FontSize(9).LetterSpacing(0.15f);
            });

            cash.Item().Padding(6).Column(body =>
            {
                body.Item().Row(r =>
                {
                    r.RelativeItem().Text("COD Amount:").Bold().FontSize(9);
                    r.ConstantItem(70).AlignRight().Text(codAmount.ToString("N2")).Bold().FontSize(9);
                });
                body.Item().PaddingTop(2).Row(r =>
                {
                    r.RelativeItem().Text("Duty Amount:").Bold().FontSize(9);
                    r.ConstantItem(70).AlignRight().Text(dutyAmount.ToString("N2")).Bold().FontSize(9);
                });
                body.Item().PaddingTop(4).BorderTop(1).PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text($"TOTAL AMOUNT ({currency})").Bold().FontSize(10);
                    r.ConstantItem(70).AlignRight().Text(totalAmount.ToString("N2")).Bold().FontSize(12);
                });
            });
        });
    }

    private void A5PodFooter(ColumnDescriptor column, InscanMaster shipment, string? website = null)
    {
        var accountCode = shipment.CustomerId != null ? $"Account: {shipment.CustomerId}" : "";
        var serviceType = GetMovementTypeDisplay(shipment.MovementTypeId);

        column.Item().PaddingTop(4).BorderTop(2).PaddingTop(3).Column(footer =>
        {
            footer.Item().Row(podHeaderRow =>
            {
                podHeaderRow.RelativeItem().Column(leftPod =>
                {
                    leftPod.Item().Text("Item Description / Special Instruction").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                    var footerDescParts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(shipment.CargoDescription))
                        footerDescParts.Add(shipment.CargoDescription);
                    if (!string.IsNullOrWhiteSpace(shipment.Remarks))
                        footerDescParts.Add(shipment.Remarks);
                    leftPod.Item().PaddingTop(2).Text(string.Join(" | ", footerDescParts)).FontSize(8);
                });
                podHeaderRow.RelativeItem().AlignRight().Column(rightPod =>
                {
                    rightPod.Item().AlignRight().Text("Proof of Delivery").Bold().FontSize(9);
                    rightPod.Item().PaddingTop(2).AlignRight().Text("Consignment Received in Good Condition").FontSize(7).FontColor(Colors.Grey.Darken1);
                });
            });

            footer.Item().PaddingTop(4).Row(podRow =>
            {
                podRow.RelativeItem().PaddingRight(12).Column(colBy =>
                {
                    colBy.Item().Text("Collected By (Name & Signature)").FontSize(7).FontColor(Colors.Grey.Darken1);
                    colBy.Item().PaddingTop(10).BorderBottom(0.5f);
                    colBy.Item().PaddingTop(2).Text(text =>
                    {
                        text.Span("Service Type : ").FontSize(7).FontColor(Colors.Grey.Darken1);
                        text.Span(serviceType.ToUpper()).Bold().FontSize(8);
                    });
                    colBy.Item().PaddingTop(2).Text("Date & Time").FontSize(7).FontColor(Colors.Grey.Darken1);
                    colBy.Item().PaddingTop(5).BorderBottom(0.5f);
                });
                podRow.RelativeItem().Column(delBy =>
                {
                    delBy.Item().Text("Name & Signature").FontSize(7).FontColor(Colors.Grey.Darken1);
                    delBy.Item().PaddingTop(10).BorderBottom(0.5f);
                    delBy.Item().PaddingTop(2).Text("Date & Time").FontSize(7).FontColor(Colors.Grey.Darken1);
                    delBy.Item().PaddingTop(5).BorderBottom(0.5f);
                    delBy.Item().PaddingTop(2).Text("Delivered By (Name & Signature)").FontSize(7).FontColor(Colors.Grey.Darken1);
                    delBy.Item().PaddingTop(5).BorderBottom(0.5f);
                    delBy.Item().PaddingTop(2).Text("Date & Time").FontSize(7).FontColor(Colors.Grey.Darken1);
                });
            });

            footer.Item().PaddingTop(3).Row(bottomRow =>
            {
                bottomRow.RelativeItem().Text(text =>
                {
                    if (!string.IsNullOrEmpty(accountCode))
                    {
                        text.Span(accountCode).Bold().FontSize(8).FontColor("#1e3a5f");
                        text.Span("     ");
                    }
                    if (!string.IsNullOrEmpty(website))
                    {
                        text.Span(website).FontSize(8).FontColor("#1e3a5f");
                    }
                });
                bottomRow.ConstantItem(50).AlignRight().Text("Page 1/1").Bold().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    public byte[] GenerateBulkA5AWB(List<InscanMaster> shipments, string? companyName = null, byte[]? logoData = null, string? website = null, string? branchCurrency = null)
    {
        var effectiveLogo = logoData ?? _logoData;
        var document = Document.Create(container =>
        {
            foreach (var shipment in shipments)
            {
                var currency = branchCurrency ?? shipment.Currency ?? "AED";
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape());
                    page.MarginHorizontal(16);
                    page.MarginVertical(12);
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                    page.Content().Column(column =>
                    {
                        column.Spacing(0);
                        A5Header(column, shipment, companyName ?? "Net4Courier", effectiveLogo);
                        A5InfoRow(column, shipment);
                        A5MiddleSection(column, shipment, currency);
                        A5PodFooter(column, shipment, website);
                    });
                });
            }
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateBulkLabel(List<InscanMaster> shipments, string? companyName = null, byte[]? logoData = null)
    {
        var effectiveLogo = logoData ?? _logoData;
        var document = Document.Create(container =>
        {
            foreach (var shipment in shipments)
            {
                container.Page(page =>
                {
                    page.Size(100, 150, Unit.Millimetre);
                    page.Margin(0);
                    page.DefaultTextStyle(x => x.FontSize(6).FontFamily("Arial"));

                    page.Content().Border(1.5f).Column(column =>
                    {
                        column.Spacing(0);

                        LabelHeader(column, shipment);
                        LabelBarcode(column, shipment);
                        LabelShipper(column, shipment);
                        LabelReceiver(column, shipment);
                        LabelMetrics(column, shipment);
                        LabelFooter(column, shipment);
                    });
                });
            }
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateLabel(InscanMaster shipment, string? companyName = null, byte[]? logoData = null)
    {
        var effectiveLogo = logoData ?? _logoData;
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(100, 150, Unit.Millimetre);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontSize(6).FontFamily("Arial"));

                page.Content().Border(1.5f).Column(column =>
                {
                    column.Spacing(0);

                    LabelHeader(column, shipment);
                    LabelBarcode(column, shipment);
                    LabelShipper(column, shipment);
                    LabelReceiver(column, shipment);
                    LabelMetrics(column, shipment);
                    LabelFooter(column, shipment);
                });
            });
        });

        return document.GeneratePdf();
    }

    private string GetPortCode(string? portCode, string? city)
    {
        if (!string.IsNullOrWhiteSpace(portCode)) return portCode;
        if (!string.IsNullOrWhiteSpace(city) && city.Length >= 3) return city.Substring(0, 3).ToUpper();
        if (!string.IsNullOrWhiteSpace(city)) return city.ToUpper();
        return "---";
    }

    private static string GetMovementTypeDisplay(MovementType movementType)
    {
        return movementType switch
        {
            MovementType.Domestic => "Domestic",
            MovementType.InternationalExport => "International Export",
            MovementType.InternationalImport => "Import",
            MovementType.Transhipment => "Transhipment",
            _ => "E-Commerce Delivery"
        };
    }

    private string GetCountryDisplayCode(string? portCode, string? country)
    {
        if (!string.IsNullOrWhiteSpace(portCode)) return portCode;
        if (!string.IsNullOrWhiteSpace(country) && country.Length > 3)
            return country.Substring(0, 3).ToUpper();
        if (!string.IsNullOrWhiteSpace(country)) return country.ToUpper();
        return "---";
    }

    private static string CombinePhoneNumbers(string? phone, string? mobile)
    {
        var hasPhone = !string.IsNullOrWhiteSpace(phone);
        var hasMobile = !string.IsNullOrWhiteSpace(mobile);
        if (hasPhone && hasMobile && phone!.Trim() != mobile!.Trim())
            return $"{phone!.Trim()} / {mobile!.Trim()}";
        if (hasPhone) return phone!.Trim();
        if (hasMobile) return mobile!.Trim();
        return "";
    }

    private static string Truncate(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "..";
    }

    private void LabelHeader(ColumnDescriptor column, InscanMaster shipment)
    {
        var isExport = shipment.MovementTypeId == MovementType.InternationalExport || shipment.MovementTypeId == MovementType.InternationalImport;
        var movementLabel = isExport ? "INTERNATIONAL" : "DOMESTIC";

        column.Item().BorderBottom(1.5f).PaddingHorizontal(6).PaddingVertical(4).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text(movementLabel).Bold().FontSize(11);
                c.Item().Text(shipment.CargoDescription ?? "E-COMMERCE DELIVERY").Bold().FontSize(6);
            });
            row.ConstantItem(58).AlignRight().Column(c =>
            {
                c.Item().AlignRight().Text("DATE").Bold().FontSize(6);
                c.Item().AlignRight().Text(shipment.TransactionDate.ToString("dd/MM/yyyy")).Bold().FontSize(8);
            });
        });
    }

    private void LabelBarcode(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderBottom(1.5f).PaddingVertical(3).AlignCenter().Column(c =>
        {
            if (shipment.BarcodeImage != null)
            {
                c.Item().AlignCenter().MaxHeight(38).MaxWidth(220).Image(shipment.BarcodeImage).FitArea();
            }
            c.Item().PaddingTop(1).AlignCenter().Text(shipment.AWBNo).Bold().FontSize(12).LetterSpacing(0.1f);
        });
    }

    private void LabelShipper(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderBottom(1).Background("#f9fafb").PaddingHorizontal(6).PaddingVertical(3).Column(c =>
        {
            c.Item().Row(shipperLabelRow =>
            {
                shipperLabelRow.AutoItem().Background("#e2e8f0").PaddingHorizontal(4).PaddingVertical(1)
                    .Text("SHIPPER").Bold().FontSize(6);
                shipperLabelRow.RelativeItem();
            });
            c.Item().Height(2);
            c.Item().Row(row =>
            {
                row.RelativeItem().Column(sc =>
                {
                    sc.Item().Text(Truncate(shipment.Consignor, 40)).Bold().FontSize(7);
                    var shipperAddr = $"{shipment.ConsignorAddress1} {shipment.ConsignorAddress2}".Trim();
                    if (!string.IsNullOrWhiteSpace(shipperAddr))
                        sc.Item().Text(Truncate(shipperAddr, 50)).FontSize(6);
                });
                row.ConstantItem(75).AlignRight().AlignMiddle().Text(CombinePhoneNumbers(shipment.ConsignorPhone, shipment.ConsignorMobile)).Bold().FontSize(7);
            });
        });
    }

    private void LabelReceiver(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderBottom(1.5f).PaddingHorizontal(6).PaddingVertical(4).Column(c =>
        {
            c.Item().Background(Colors.Black).PaddingHorizontal(4).PaddingVertical(2)
                .Text("DELIVER TO").FontColor(Colors.White).Bold().FontSize(7);
            c.Item().Height(3);
            c.Item().Text(Truncate(shipment.Consignee, 40)).Bold().FontSize(12);

            var fullAddr = $"{shipment.ConsigneeAddress1} {shipment.ConsigneeAddress2}".Trim();
            if (!string.IsNullOrWhiteSpace(fullAddr))
                c.Item().PaddingTop(1).Text(Truncate(fullAddr, 80)).FontSize(7);
            if (!string.IsNullOrWhiteSpace(shipment.ConsigneeLocation))
                c.Item().Text(shipment.ConsigneeLocation).FontSize(7);

            c.Item().Height(3);
            c.Item().Row(row =>
            {
                row.RelativeItem().Column(lc =>
                {
                    lc.Item().Text(shipment.ConsigneeCity ?? "").Bold().FontSize(16);
                    lc.Item().Height(1);
                    var phone = CombinePhoneNumbers(shipment.ConsigneePhone, shipment.ConsigneeMobile);
                    if (!string.IsNullOrWhiteSpace(phone))
                        lc.Item().Text($"Tel: {phone}").Bold().FontSize(8);
                });
                row.ConstantItem(50).AlignRight().Column(rc =>
                {
                    rc.Item().AlignRight().Text("ORIGIN").Bold().FontSize(6);
                    rc.Item().AlignRight().Text(GetPortCode(shipment.OriginPortCode, shipment.ConsignorCity)).Bold().FontSize(14);
                });
            });
        });
    }

    private void LabelMetrics(ColumnDescriptor column, InscanMaster shipment)
    {
        var hasDuty = (shipment.DutyVatAmount ?? 0) > 0;
        var paymentMode = hasDuty ? "DDP" : "DDU";

        column.Item().BorderBottom(1.5f).Background("#f9fafb").Height(32).Row(row =>
        {
            row.RelativeItem().BorderRight(1).PaddingVertical(3).AlignCenter().AlignMiddle().Column(c =>
            {
                c.Item().AlignCenter().Text("WEIGHT").Bold().FontSize(5);
                c.Item().AlignCenter().Text($"{shipment.Weight?.ToString("F1") ?? "0.0"} KG").Bold().FontSize(9);
            });
            row.RelativeItem().BorderRight(1).PaddingVertical(3).AlignCenter().AlignMiddle().Column(c =>
            {
                c.Item().AlignCenter().Text("PIECES").Bold().FontSize(5);
                c.Item().AlignCenter().Text($"{shipment.Pieces ?? 1} of {shipment.Pieces ?? 1}").Bold().FontSize(9);
            });
            row.RelativeItem().BorderRight(1).PaddingVertical(3).AlignCenter().AlignMiddle().Column(c =>
            {
                c.Item().AlignCenter().Text("INCO TERMS").Bold().FontSize(5);
                c.Item().AlignCenter().Text(paymentMode).Bold().FontSize(9).FontColor("#1565c0");
            });
            row.RelativeItem().PaddingVertical(3).PaddingHorizontal(2).AlignCenter().AlignMiddle().Column(c =>
            {
                c.Item().AlignCenter().Text("REFERENCE").Bold().FontSize(5);
                c.Item().AlignCenter().Text(Truncate(shipment.ReferenceNo ?? shipment.AWBNo ?? "", 12)).Bold().FontSize(6);
            });
        });
    }

    private void LabelFooter(ColumnDescriptor column, InscanMaster shipment)
    {
        var currency = shipment.Currency ?? "AED";
        var codAmount = shipment.IsCOD ? (shipment.CODAmount ?? 0) : 0;
        var vatAmount = shipment.TaxAmount ?? 0;
        var dutyAmount = shipment.DutyVatAmount ?? 0;
        var totalAmount = codAmount + vatAmount + dutyAmount;

        column.Item().Background(Colors.Black).PaddingHorizontal(6).PaddingVertical(4).Column(c =>
        {
            c.Item().Row(row =>
            {
                row.RelativeItem().AlignMiddle().Text("Collect Payment").FontColor(Colors.White).Bold().FontSize(8);
                row.ConstantItem(100).Column(bd =>
                {
                    bd.Item().BorderBottom(0.5f).BorderColor("#4b5563").PaddingBottom(1).Row(r =>
                    {
                        r.RelativeItem().Text("COD Amount:").FontColor(Colors.White).Bold().FontSize(7);
                        r.ConstantItem(40).AlignRight().Text(codAmount > 0 ? codAmount.ToString("N2") : "0.00").FontColor(Colors.White).FontSize(7);
                    });
                    bd.Item().PaddingTop(1).Row(r =>
                    {
                        r.RelativeItem().Text("Duty Amount:").FontColor(Colors.White).Bold().FontSize(7);
                        r.ConstantItem(40).AlignRight().Text(dutyAmount > 0 ? dutyAmount.ToString("N2") : "0.00").FontColor(Colors.White).FontSize(7);
                    });
                });
            });
            c.Item().Height(3);
            c.Item().BorderTop(0.5f).BorderColor("#4b5563").PaddingTop(2).Row(row =>
            {
                var hasDuty = dutyAmount > 0;
                var incoLabel = hasDuty ? "DDP: Delivered Duty Paid" : "DDU: Delivered Duty Unpaid";
                row.RelativeItem().AlignBottom().Text(incoLabel).FontColor("#9ca3af").Bold().FontSize(5);
                row.ConstantItem(100).AlignRight().Column(tc =>
                {
                    tc.Item().AlignRight().Text($"TOTAL ({currency})").FontColor(Colors.White).Bold().FontSize(7);
                    tc.Item().PaddingTop(1).AlignRight().Text(totalAmount > 0 ? totalAmount.ToString("N2") : "0.00").FontColor(Colors.White).Bold().FontSize(18);
                });
            });
        });
    }

    private void BuildLabelHeader(ColumnDescriptor column, InscanMaster shipment, string companyName, byte[]? logoData = null)
    {
        var effectiveLogo = logoData ?? _logoData;
        column.Item().Row(row =>
        {
            row.RelativeItem(1).Padding(3).Column(c =>
            {
                if (effectiveLogo != null)
                {
                    c.Item().Height(25).Image(effectiveLogo).FitHeight();
                }
                else
                {
                    c.Item().Text(companyName).Bold().FontSize(12).FontColor(Colors.Red.Darken2);
                }
            });
            
            row.RelativeItem(2).BorderLeft(1).Padding(3).Column(c =>
            {
                if (shipment.BarcodeImage != null)
                {
                    c.Item().AlignCenter().Height(30).Image(shipment.BarcodeImage);
                }
                c.Item().AlignCenter().Text(shipment.AWBNo).Bold().FontSize(9);
            });
        });
    }

    private void BuildLabelOriginDest(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Row(row =>
        {
            row.RelativeItem(1).BorderRight(1).Padding(3).Column(c =>
            {
                c.Item().Text("Origin:").FontSize(6);
                c.Item().Text(shipment.OriginPortCode ?? shipment.ConsignorCity ?? "---").Bold().FontSize(16);
            });
            
            row.RelativeItem(2).Padding(3).Row(destRow =>
            {
                destRow.RelativeItem().Column(c =>
                {
                    c.Item().Text("Destination:").FontSize(6);
                    c.Item().Text(shipment.DestinationPortCode ?? shipment.ConsigneeCity ?? "---").Bold().FontSize(16);
                });
                destRow.RelativeItem().Column(c =>
                {
                    c.Item().Text("Date:").FontSize(6);
                    c.Item().Text(shipment.TransactionDate.ToString("MMM d, yyyy")).FontSize(7);
                    c.Item().Height(2);
                    c.Item().Text("Ref:").FontSize(6);
                    c.Item().Text(shipment.ReferenceNo ?? "").FontSize(6);
                });
            });
        });
    }

    private void BuildLabelService(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Row(row =>
        {
            row.ConstantItem(40).BorderRight(1).Padding(2).Column(c =>
            {
                c.Item().AlignCenter().Text(shipment.MovementTypeId == MovementType.InternationalExport || shipment.MovementTypeId == MovementType.InternationalImport ? "EXP" : "DOM").Bold().FontSize(14);
            });
            row.ConstantItem(40).BorderRight(1).Padding(2).Column(c =>
            {
                c.Item().AlignCenter().Text("PPX").Bold().FontSize(14);
            });
            row.ConstantItem(25).BorderRight(1).Padding(2).Column(c =>
            {
                c.Item().AlignCenter().Text(shipment.DocumentTypeId == Kernel.Enums.DocumentType.Document ? "D" : "P").Bold().FontSize(14);
            });
            row.RelativeItem().Padding(2).Column(c =>
            {
                c.Item().AlignCenter().Text(shipment.Pieces?.ToString() ?? "1").Bold().FontSize(14);
            });
            
            if (shipment.IsCOD)
            {
                row.ConstantItem(35).BorderLeft(1).Background(Colors.Yellow.Lighten3).Padding(2).Column(c =>
                {
                    c.Item().AlignCenter().Text("COD").Bold().FontSize(10);
                    c.Item().AlignCenter().Text(shipment.CODAmount?.ToString("F0") ?? "0").Bold().FontSize(8);
                });
            }
        });
    }

    private void BuildLabelWeight(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Row(row =>
        {
            row.RelativeItem().Padding(2).Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.RelativeItem().Text($"Weight: {shipment.Weight?.ToString("F2") ?? "0.00"} KG").FontSize(7);
                    r.RelativeItem().Text($"Chargeable: {shipment.ChargeableWeight?.ToString("F2") ?? "0.00"} KG").FontSize(7);
                });
            });
            row.RelativeItem().BorderLeft(1).Padding(2).Column(c =>
            {
                c.Item().Text($"Services: {(shipment.CustomsValue > 0 ? $"Customs: {shipment.CustomsValue} {shipment.Currency}" : "")}").FontSize(6);
            });
        });
    }

    private void BuildLabelShipper(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(3).Column(c =>
        {
            c.Item().Row(r =>
            {
                r.ConstantItem(50).Text("Account:").FontSize(6);
                r.RelativeItem().Text(shipment.CustomerId?.ToString() ?? "").FontSize(7);
            });
            c.Item().Text(shipment.Consignor ?? "").Bold().FontSize(8);
            c.Item().Text(shipment.Consignor ?? "").FontSize(7);
            c.Item().Text($"{shipment.ConsignorAddress1} {shipment.ConsignorAddress2}".Trim()).FontSize(7);
            c.Item().Row(r =>
            {
                r.RelativeItem().Text($"{shipment.ConsignorCity}").FontSize(7);
                r.RelativeItem().Text(shipment.ConsignorPostalCode ?? "").FontSize(7);
            });
            c.Item().Row(r =>
            {
                r.RelativeItem().Text(GetCountryDisplayCode(null, shipment.ConsignorCountry)).FontSize(7);
                r.RelativeItem().Text(shipment.ConsignorPhone ?? shipment.ConsignorMobile ?? "").FontSize(7);
            });
        });
    }

    private void BuildLabelConsignee(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(2).Background(Colors.Grey.Lighten4).Padding(3).Column(c =>
        {
            c.Item().Text(shipment.Consignee ?? "").Bold().FontSize(9);
            c.Item().Text(shipment.Consignee ?? "").FontSize(7);
            c.Item().Text($"{shipment.ConsigneeAddress1} {shipment.ConsigneeAddress2}".Trim()).FontSize(7);
            c.Item().Height(3);
            c.Item().Row(r =>
            {
                r.RelativeItem().Text(shipment.ConsigneeCity ?? "").Bold().FontSize(9);
            });
            c.Item().Row(r =>
            {
                r.RelativeItem().Text(GetCountryDisplayCode(null, shipment.ConsigneeCountry)).FontSize(7);
            });
            c.Item().Row(r =>
            {
                r.RelativeItem().Text(CombinePhoneNumbers(shipment.ConsigneePhone, shipment.ConsigneeMobile)).Bold().FontSize(8);
            });
        });
    }

    private void BuildLabelRoute(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Row(row =>
        {
            row.RelativeItem().Text($"Route: {shipment.DestinationPortCode ?? "N/A"}").FontSize(7);
        });
    }

    private void BuildLabelDescription(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Column(c =>
        {
            c.Item().Text("Description:").FontSize(6);
            c.Item().Text(shipment.CargoDescription ?? "Documents").FontSize(7);
        });
    }

    private void BuildLabelReferences(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text($"Shipper Ref: {shipment.ReferenceNo ?? ""}").FontSize(6);
            });
            row.RelativeItem().Column(c =>
            {
                c.Item().Text($"Consignee Ref: {shipment.Consignee ?? ""}").FontSize(6);
            });
        });
    }

    public byte[] GenerateShipmentInvoice(InscanMaster shipment, byte[]? branchLogoData = null, string invoiceNo = "")
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));
                
                page.Content().Column(column =>
                {
                    column.Spacing(10);
                    
                    BuildShipmentInvoiceHeader(column, shipment, branchLogoData, invoiceNo);
                    BuildShipmentParties(column, shipment);
                    BuildShipmentItemsTable(column, shipment);
                    BuildShipmentSummary(column, shipment);
                });
            });
        });
        
        return document.GeneratePdf();
    }

    private void BuildShipmentInvoiceHeader(ColumnDescriptor column, InscanMaster shipment, byte[]? logoData, string invoiceNo)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem(1).Column(logoCol =>
            {
                if (logoData != null && logoData.Length > 0)
                {
                    logoCol.Item().Height(60).Image(logoData).FitHeight();
                }
                else if (_logoData != null)
                {
                    logoCol.Item().Height(60).Image(_logoData).FitHeight();
                }
                else
                {
                    logoCol.Item().Text("GateEx").Bold().FontSize(24).FontColor(Colors.Blue.Darken2);
                }
            });
            
            row.RelativeItem(1).AlignRight().Column(infoCol =>
            {
                infoCol.Item().Text($"AWB No.  {shipment.AWBNo}").Bold().FontSize(11);
                infoCol.Item().Height(10);
                infoCol.Item().Text($"DATE: {shipment.TransactionDate:dd/MM/yyyy}").FontSize(10);
                infoCol.Item().Height(5);
                infoCol.Item().Text($"INVOICE No. {invoiceNo}").FontSize(10);
            });
        });
        
        column.Item().Height(15);
        
        column.Item().Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text($"SHIPMENT FROM: {GetCountryDisplayCode(null, shipment.ConsignorCountry)}").Bold().FontSize(10);
            });
            row.RelativeItem().Column(c =>
            {
                c.Item().Text($"SHIPMENT TO: {GetCountryDisplayCode(null, shipment.ConsigneeCountry)}").Bold().FontSize(10);
            });
        });
    }

    private void BuildShipmentParties(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().Border(1).Row(row =>
        {
            row.RelativeItem(1).BorderRight(0.5f).Padding(10).Column(shipperCol =>
            {
                shipperCol.Item().Row(r =>
                {
                    r.ConstantItem(50).Text("Name:").FontSize(9);
                    r.RelativeItem().Text(shipment.Consignor ?? "").FontSize(9);
                });
                shipperCol.Item().Height(5);
                shipperCol.Item().Row(r =>
                {
                    r.ConstantItem(50).Text("Address:").FontSize(9);
                    r.RelativeItem().Text($"{shipment.ConsignorAddress1} {shipment.ConsignorAddress2}".Trim()).FontSize(9);
                });
                shipperCol.Item().Height(5);
                shipperCol.Item().Row(r =>
                {
                    r.ConstantItem(50).Text("City:").FontSize(9);
                    r.RelativeItem().Text(shipment.ConsignorCity ?? "").FontSize(9);
                });
                shipperCol.Item().Row(r =>
                {
                    r.ConstantItem(50).Text("Country:").FontSize(9);
                    r.RelativeItem().Text(GetCountryDisplayCode(null, shipment.ConsignorCountry)).FontSize(9);
                });
                shipperCol.Item().Height(5);
                shipperCol.Item().Row(r =>
                {
                    r.ConstantItem(50).Text("Tel.:").FontSize(9);
                    r.RelativeItem().Text(shipment.ConsignorPhone ?? shipment.ConsignorMobile ?? "").FontSize(9);
                });
            });
            
            row.RelativeItem(1).Padding(10).Column(consigneeCol =>
            {
                consigneeCol.Item().Row(r =>
                {
                    r.ConstantItem(80).Text("Name:").FontSize(9);
                    r.RelativeItem().Text(shipment.Consignee ?? "").FontSize(9);
                });
                consigneeCol.Item().Height(5);
                consigneeCol.Item().Row(r =>
                {
                    r.ConstantItem(80).Text("Address:").FontSize(9);
                    r.RelativeItem().Text($"{shipment.ConsigneeAddress1} {shipment.ConsigneeAddress2}".Trim()).FontSize(9);
                });
                consigneeCol.Item().Height(5);
                consigneeCol.Item().Row(r =>
                {
                    r.ConstantItem(80).Text("City/Postal Code:").FontSize(9);
                    r.RelativeItem().Text($"{shipment.ConsigneeCity ?? ""} {shipment.ConsigneePostalCode ?? ""}".Trim()).FontSize(9);
                });
                consigneeCol.Item().Row(r =>
                {
                    r.ConstantItem(80).Text("Country:").FontSize(9);
                    r.RelativeItem().Text(GetCountryDisplayCode(null, shipment.ConsigneeCountry)).FontSize(9);
                });
                consigneeCol.Item().Height(5);
                consigneeCol.Item().Row(r =>
                {
                    r.ConstantItem(80).Text("Tel./Fax No.:").FontSize(9);
                    r.RelativeItem().Text(CombinePhoneNumbers(shipment.ConsigneePhone, shipment.ConsigneeMobile)).FontSize(9);
                });
            });
        });
    }

    private void BuildShipmentItemsTable(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(3);
                cols.RelativeColumn(1.5f);
                cols.RelativeColumn(1);
                cols.RelativeColumn(1.5f);
                cols.RelativeColumn(1.2f);
                cols.RelativeColumn(1.2f);
            });
            
            table.Header(header =>
            {
                header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(5).Text("Description of goods").Bold().FontSize(9);
                header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(5).Text("HS CODE").Bold().FontSize(9);
                header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text("Quantity").Bold().FontSize(9);
                header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(5).Text("Country of origin").Bold().FontSize(9);
                header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"Unit Value\n{shipment.Currency ?? "AED"}").Bold().FontSize(9);
                header.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"Total value\n{shipment.Currency ?? "AED"}").Bold().FontSize(9);
            });
            
            var unitValue = (shipment.CustomsValue ?? 0) / (shipment.Pieces ?? 1);
            table.Cell().Border(1).Padding(5).Text(shipment.CargoDescription ?? "").FontSize(9);
            table.Cell().Border(1).Padding(5).Text("").FontSize(9);
            table.Cell().Border(1).Padding(5).AlignCenter().Text((shipment.Pieces ?? 1).ToString()).FontSize(9);
            table.Cell().Border(1).Padding(5).Text(shipment.ConsignorCountry ?? "").FontSize(9);
            table.Cell().Border(1).Padding(5).AlignRight().Text(unitValue.ToString("F2")).FontSize(9);
            table.Cell().Border(1).Padding(5).AlignRight().Text((shipment.CustomsValue ?? 0).ToString("F2")).FontSize(9);
            
            for (int i = 0; i < 8; i++)
            {
                table.Cell().Border(1).Padding(5).Height(18).Text("").FontSize(9);
                table.Cell().Border(1).Padding(5).Height(18).Text("").FontSize(9);
                table.Cell().Border(1).Padding(5).Height(18).Text("").FontSize(9);
                table.Cell().Border(1).Padding(5).Height(18).Text("").FontSize(9);
                table.Cell().Border(1).Padding(5).Height(18).Text("").FontSize(9);
                table.Cell().Border(1).Padding(5).Height(18).Text("").FontSize(9);
            }
            
            table.Cell().ColumnSpan(5).Border(1).Padding(5).AlignRight().Text($"Total value in {shipment.Currency ?? "AED"}").Bold().FontSize(9);
            table.Cell().Border(1).Padding(5).AlignRight().Text((shipment.CustomsValue ?? 0).ToString("F2")).Bold().FontSize(9);
        });
    }

    private void BuildShipmentSummary(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().Height(10);
        
        column.Item().Border(1).Padding(10).Column(summaryCol =>
        {
            summaryCol.Item().Row(r =>
            {
                r.RelativeItem().Text($"Number of pieces: {shipment.Pieces ?? 1}").FontSize(9);
            });
            summaryCol.Item().Height(3);
            summaryCol.Item().Row(r =>
            {
                r.RelativeItem().Text($"Total Gross Weight: {shipment.Weight?.ToString("F2") ?? "0.00"}kg").FontSize(9);
            });
            summaryCol.Item().Height(3);
            summaryCol.Item().Row(r =>
            {
                r.RelativeItem().Text($"Total Net Weight: {((shipment.Weight ?? 0) * 0.9m):F2}").FontSize(9);
            });
            summaryCol.Item().Height(3);
            summaryCol.Item().Row(r =>
            {
                r.RelativeItem().Text($"Type: {(shipment.DocumentTypeId == DocumentType.Document ? "Docs" : "Non-Docs")}").FontSize(9);
            });
            summaryCol.Item().Height(3);
            summaryCol.Item().Row(r =>
            {
                r.RelativeItem().Text("Term of transportation: Air Express").FontSize(9);
            });
            summaryCol.Item().Height(3);
            summaryCol.Item().Row(r =>
            {
                r.RelativeItem().Text("Reason for Export: Final Export").FontSize(9);
            });
        });
    }

    public byte[] GenerateImportTrackingReport(ImportShipment shipment, DateTime transactionDate, byte[]? logoData = null)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));
                
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            if (logoData != null)
                            {
                                c.Item().Width(120).Image(logoData);
                            }
                            else
                            {
                                c.Item().Text("Net4Courier").Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                            }
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("SHIPMENT TRACKING").Bold().FontSize(16).FontColor(Colors.Grey.Darken3);
                            c.Item().Text("Import Shipment").FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                    });
                    col.Item().Height(15);
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().Height(15);
                });
                
                page.Content().Column(column =>
                {
                    column.Spacing(15);
                    
                    column.Item().Background(Colors.Blue.Lighten5).Padding(15).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("AWB NUMBER").FontSize(9).FontColor(Colors.Grey.Darken1).Bold();
                            c.Item().Height(3);
                            c.Item().Text(shipment.AWBNo ?? "N/A").Bold().FontSize(18).FontColor(Colors.Blue.Darken3);
                        });
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("DATE").FontSize(9).FontColor(Colors.Grey.Darken1).Bold();
                            c.Item().Height(3);
                            c.Item().Text(transactionDate.ToString("dd MMM yyyy")).FontSize(12);
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("STATUS").FontSize(9).FontColor(Colors.Grey.Darken1).Bold();
                            c.Item().Height(3);
                            var statusColor = shipment.Status switch
                            {
                                ImportShipmentStatus.HandedOver => Colors.Green.Darken2,
                                ImportShipmentStatus.Released => Colors.Blue.Darken2,
                                ImportShipmentStatus.Cleared => Colors.Blue.Lighten1,
                                ImportShipmentStatus.Expected => Colors.Orange.Darken2,
                                ImportShipmentStatus.CustomsHold or ImportShipmentStatus.OnHold => Colors.Red.Darken2,
                                _ => Colors.Grey.Darken2
                            };
                            c.Item().Text(shipment.Status.ToString()).Bold().FontSize(14).FontColor(statusColor);
                        });
                    });
                    
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(10).Height(10).Background(Colors.Grey.Lighten1);
                                r.ConstantItem(10);
                                r.RelativeItem().Text("FROM (SHIPPER)").Bold().FontSize(9).FontColor(Colors.Grey.Darken2);
                            });
                            c.Item().Height(8);
                            c.Item().Text(shipment.ShipperName ?? "N/A").Bold().FontSize(11);
                            c.Item().Height(4);
                            c.Item().Text(shipment.ShipperAddress ?? "").FontSize(9).FontColor(Colors.Grey.Darken1);
                            c.Item().Height(2);
                            c.Item().Text($"{shipment.ShipperCity ?? ""}, {shipment.ShipperCountry ?? ""}".Trim().TrimEnd(',')).FontSize(9).FontColor(Colors.Grey.Darken1);
                            c.Item().Height(4);
                            if (!string.IsNullOrEmpty(shipment.ShipperPhone))
                            {
                                c.Item().Text($"Tel: {shipment.ShipperPhone}").FontSize(8).FontColor(Colors.Grey.Medium);
                            }
                        });
                        row.ConstantItem(20);
                        row.RelativeItem().Border(1).BorderColor(Colors.Blue.Lighten3).Padding(15).Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(10).Height(10).Background(Colors.Blue.Darken2);
                                r.ConstantItem(10);
                                r.RelativeItem().Text("TO (CONSIGNEE)").Bold().FontSize(9).FontColor(Colors.Blue.Darken2);
                            });
                            c.Item().Height(8);
                            c.Item().Text(shipment.ConsigneeName ?? "N/A").Bold().FontSize(11);
                            c.Item().Height(4);
                            c.Item().Text(shipment.ConsigneeAddress ?? "").FontSize(9).FontColor(Colors.Grey.Darken1);
                            c.Item().Height(2);
                            c.Item().Text($"{shipment.ConsigneeCity ?? ""}, {shipment.ConsigneePostalCode ?? ""} {shipment.ConsigneeCountry ?? ""}".Trim().TrimEnd(',')).FontSize(9).FontColor(Colors.Grey.Darken1);
                            c.Item().Height(4);
                            if (!string.IsNullOrEmpty(shipment.ConsigneePhone) || !string.IsNullOrEmpty(shipment.ConsigneeMobile))
                            {
                                c.Item().Text($"Tel: {shipment.ConsigneePhone ?? shipment.ConsigneeMobile}").FontSize(8).FontColor(Colors.Grey.Medium);
                            }
                        });
                    });
                    
                    column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });
                        void AddCell(string label, string value)
                        {
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Column(c =>
                            {
                                c.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1).Bold();
                                c.Item().Height(3);
                                c.Item().Text(value).FontSize(10);
                            });
                        }
                        AddCell("PIECES", shipment.Pieces.ToString());
                        AddCell("WEIGHT", $"{shipment.Weight:F2} kg");
                        AddCell("VOLUME WEIGHT", $"{shipment.VolumetricWeight?.ToString("F2") ?? "0.00"} kg");
                        AddCell("CHARGEABLE WEIGHT", $"{shipment.ChargeableWeight?.ToString("F2") ?? "0.00"} kg");
                        AddCell("SHIPMENT TYPE", shipment.ShipmentType.ToString());
                        AddCell("IMPORT TYPE", shipment.ImportType.ToString());
                        AddCell("PAYMENT MODE", shipment.PaymentMode.ToString());
                        AddCell("COD AMOUNT", shipment.CODAmount?.ToString("F2") ?? "0.00");
                        if (!string.IsNullOrEmpty(shipment.ReferenceNo))
                        {
                            AddCell("REFERENCE", shipment.ReferenceNo);
                            AddCell("CUSTOMS STATUS", shipment.CustomsStatus.ToString());
                        }
                    });
                    
                    if (shipment.DutyAmount > 0 || shipment.VATAmount > 0 || shipment.OtherCharges > 0)
                    {
                        column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(c =>
                        {
                            c.Item().Text("CUSTOMS & DUTY CHARGES").Bold().FontSize(11).FontColor(Colors.Grey.Darken3);
                            c.Item().Height(8);
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"Duty: {shipment.DutyAmount?.ToString("F2") ?? "0.00"}").FontSize(10);
                                r.RelativeItem().Text($"VAT: {shipment.VATAmount?.ToString("F2") ?? "0.00"}").FontSize(10);
                                r.RelativeItem().Text($"Other: {shipment.OtherCharges?.ToString("F2") ?? "0.00"}").FontSize(10);
                                r.RelativeItem().Text($"Total: {shipment.TotalCustomsCharges?.ToString("F2") ?? "0.00"}").Bold().FontSize(10);
                            });
                        });
                    }
                    
                    column.Item().Column(c =>
                    {
                        c.Item().Text("SHIPMENT TIMELINE").Bold().FontSize(12).FontColor(Colors.Grey.Darken3);
                        c.Item().Height(10);
                        c.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(timeCol =>
                        {
                            void AddEvent(string label, DateTime? date, string? user)
                            {
                                if (date == null) return;
                                timeCol.Item().Row(r =>
                                {
                                    r.ConstantItem(12).Height(12).AlignCenter().Background(Colors.Blue.Lighten1);
                                    r.ConstantItem(10);
                                    r.RelativeItem().Column(ec =>
                                    {
                                        ec.Item().Text(label).Bold().FontSize(10);
                                        ec.Item().Text($"{date.Value.ToLocalTime():dd MMM yyyy HH:mm}{(user != null ? $" by {user}" : "")}").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    });
                                });
                                timeCol.Item().Height(8);
                            }
                            AddEvent("Inscanned", shipment.InscannedAt, shipment.InscannedByUserName);
                            AddEvent("Customs Cleared", shipment.CustomsClearedAt, shipment.CustomsClearedByUserName);
                            AddEvent("Released", shipment.ReleasedAt, shipment.ReleasedByUserName);
                            AddEvent("Handed Over", shipment.HandedOverAt, shipment.HandedOverToUserName);
                            if (shipment.InscannedAt == null && shipment.CustomsClearedAt == null && shipment.ReleasedAt == null && shipment.HandedOverAt == null)
                            {
                                timeCol.Item().AlignCenter().Text("No timeline events recorded yet.").FontSize(10).FontColor(Colors.Grey.Darken1);
                            }
                        });
                    });
                });
                
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generated on ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).FontSize(8).FontColor(Colors.Grey.Medium);
                    t.Span(" | Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    t.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });
        
        return document.GeneratePdf();
    }

    public byte[] GenerateTrackingReport(InscanMaster shipment, List<ShipmentStatusHistory> timeline, string? serviceTypeName = null, byte[]? logoData = null)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));
                
                page.Header().Element(c => BuildTrackingHeader(c, shipment, logoData));
                
                page.Content().Column(column =>
                {
                    column.Spacing(15);
                    
                    BuildTrackingShipmentInfo(column, shipment, serviceTypeName);
                    BuildTrackingRouteSection(column, shipment);
                    BuildTrackingDetailsGrid(column, shipment);
                    BuildTrackingTimeline(column, timeline);
                });
                
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generated on ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).FontSize(8).FontColor(Colors.Grey.Medium);
                    t.Span(" | Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    t.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });
        
        return document.GeneratePdf();
    }

    private void BuildTrackingHeader(IContainer container, InscanMaster shipment, byte[]? logoData)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    if (logoData != null)
                    {
                        c.Item().Width(120).Image(logoData);
                    }
                    else
                    {
                        c.Item().Text("Net4Courier").Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                    }
                });
                
                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Text("SHIPMENT TRACKING").Bold().FontSize(16).FontColor(Colors.Grey.Darken3);
                    c.Item().Text("Proof of Shipment Status").FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });
            
            col.Item().Height(15);
            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            col.Item().Height(15);
        });
    }

    private void BuildTrackingShipmentInfo(ColumnDescriptor column, InscanMaster shipment, string? serviceTypeName)
    {
        column.Item().Background(Colors.Blue.Lighten5).Padding(15).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("AWB NUMBER").FontSize(9).FontColor(Colors.Grey.Darken1).Bold();
                c.Item().Height(3);
                c.Item().Text(shipment.AWBNo ?? "N/A").Bold().FontSize(18).FontColor(Colors.Blue.Darken3);
            });
            
            row.RelativeItem().AlignCenter().Column(c =>
            {
                c.Item().Text("BOOKING DATE").FontSize(9).FontColor(Colors.Grey.Darken1).Bold();
                c.Item().Height(3);
                c.Item().Text(shipment.TransactionDate.ToString("dd MMM yyyy")).FontSize(12);
            });
            
            row.RelativeItem().AlignRight().Column(c =>
            {
                c.Item().Text("STATUS").FontSize(9).FontColor(Colors.Grey.Darken1).Bold();
                c.Item().Height(3);
                var statusColor = shipment.CourierStatusId switch
                {
                    CourierStatus.Delivered => Colors.Green.Darken2,
                    CourierStatus.InTransit => Colors.Blue.Darken2,
                    CourierStatus.Pending => Colors.Orange.Darken2,
                    _ => Colors.Grey.Darken2
                };
                c.Item().Text(shipment.CourierStatusId.ToString()).Bold().FontSize(14).FontColor(statusColor);
            });
        });
    }

    private void BuildTrackingRouteSection(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.ConstantItem(10).Height(10).Background(Colors.Grey.Lighten1);
                    r.ConstantItem(10);
                    r.RelativeItem().Text("FROM (SHIPPER)").Bold().FontSize(9).FontColor(Colors.Grey.Darken2);
                });
                c.Item().Height(8);
                c.Item().Text(shipment.Consignor ?? "N/A").Bold().FontSize(11);
                c.Item().Height(4);
                c.Item().Text($"{shipment.ConsignorAddress1} {shipment.ConsignorAddress2}".Trim()).FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().Height(2);
                c.Item().Text($"{shipment.ConsignorCity ?? ""}, {shipment.ConsignorPostalCode ?? ""}".Trim().TrimEnd(',')).FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().Text(shipment.ConsignorCountry ?? "").FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().Height(4);
                if (!string.IsNullOrEmpty(shipment.ConsignorPhone) || !string.IsNullOrEmpty(shipment.ConsignorMobile))
                {
                    c.Item().Text($"Tel: {shipment.ConsignorPhone ?? shipment.ConsignorMobile}").FontSize(8).FontColor(Colors.Grey.Medium);
                }
            });
            
            row.ConstantItem(20);
            
            row.RelativeItem().Border(1).BorderColor(Colors.Blue.Lighten3).Padding(15).Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.ConstantItem(10).Height(10).Background(Colors.Blue.Darken2);
                    r.ConstantItem(10);
                    r.RelativeItem().Text("TO (CONSIGNEE)").Bold().FontSize(9).FontColor(Colors.Blue.Darken2);
                });
                c.Item().Height(8);
                c.Item().Text(shipment.Consignee ?? "N/A").Bold().FontSize(11);
                c.Item().Height(4);
                c.Item().Text($"{shipment.ConsigneeAddress1} {shipment.ConsigneeAddress2}".Trim()).FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().Height(2);
                c.Item().Text($"{shipment.ConsigneeCity ?? ""}, {shipment.ConsigneePostalCode ?? ""}".Trim().TrimEnd(',')).FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().Text(shipment.ConsigneeCountry ?? "").FontSize(9).FontColor(Colors.Grey.Darken1);
                c.Item().Height(4);
                if (!string.IsNullOrEmpty(shipment.ConsigneePhone) || !string.IsNullOrEmpty(shipment.ConsigneeMobile))
                {
                    c.Item().Text($"Tel: {shipment.ConsigneePhone ?? shipment.ConsigneeMobile}").FontSize(8).FontColor(Colors.Grey.Medium);
                }
            });
        });
    }

    private void BuildTrackingDetailsGrid(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn();
                cols.RelativeColumn();
                cols.RelativeColumn();
                cols.RelativeColumn();
            });
            
            void AddCell(string label, string value)
            {
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Column(c =>
                {
                    c.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1).Bold();
                    c.Item().Height(3);
                    c.Item().Text(value).FontSize(10);
                });
            }
            
            AddCell("PIECES", (shipment.Pieces ?? 1).ToString());
            AddCell("WEIGHT", $"{shipment.Weight?.ToString("F2") ?? "0.00"} kg");
            AddCell("VOLUME WEIGHT", $"{shipment.VolumetricWeight?.ToString("F2") ?? "0.00"} kg");
            AddCell("CHARGEABLE WEIGHT", $"{shipment.ChargeableWeight?.ToString("F2") ?? "0.00"} kg");
            
            AddCell("MOVEMENT TYPE", shipment.MovementTypeId.ToString());
            AddCell("DOCUMENT TYPE", shipment.DocumentTypeId.ToString());
            AddCell("PAYMENT MODE", shipment.PaymentModeId.ToString());
            AddCell("COD AMOUNT", shipment.CODAmount?.ToString("F2") ?? "0.00");
            
            if (!string.IsNullOrEmpty(shipment.ReferenceNo))
            {
                AddCell("REFERENCE", shipment.ReferenceNo);
                AddCell("", "");
            }
        });
    }

    private void BuildTrackingTimeline(ColumnDescriptor column, List<ShipmentStatusHistory> timeline)
    {
        column.Item().Column(c =>
        {
            c.Item().Text("TRACKING HISTORY").Bold().FontSize(12).FontColor(Colors.Grey.Darken3);
            c.Item().Height(10);
            
            if (timeline == null || !timeline.Any())
            {
                c.Item().Background(Colors.Grey.Lighten4).Padding(20).AlignCenter().Text("No tracking events recorded yet.").FontSize(10).FontColor(Colors.Grey.Darken1);
                return;
            }
            
            var orderedTimeline = timeline.OrderByDescending(t => t.ChangedAt).ToList();
            
            c.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Column(timelineCol =>
            {
                for (int i = 0; i < orderedTimeline.Count; i++)
                {
                    var evt = orderedTimeline[i];
                    var isFirst = i == 0;
                    var isLast = i == orderedTimeline.Count - 1;
                    
                    timelineCol.Item().Row(row =>
                    {
                        row.ConstantItem(80).Padding(10).AlignRight().Column(dateCol =>
                        {
                            dateCol.Item().Text(evt.ChangedAt.ToLocalTime().ToString("dd MMM")).FontSize(9).Bold().FontColor(Colors.Grey.Darken2);
                            dateCol.Item().Text(evt.ChangedAt.ToLocalTime().ToString("yyyy")).FontSize(8).FontColor(Colors.Grey.Medium);
                            dateCol.Item().Text(evt.ChangedAt.ToLocalTime().ToString("HH:mm")).FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                        
                        row.ConstantItem(30).AlignCenter().Column(dotCol =>
                        {
                            if (!isFirst)
                            {
                                dotCol.Item().Width(2).Height(10).Background(Colors.Grey.Lighten2);
                            }
                            else
                            {
                                dotCol.Item().Height(10);
                            }
                            
                            var dotColor = isFirst ? Colors.Green.Darken1 : Colors.Blue.Lighten1;
                            dotCol.Item().Width(12).Height(12).AlignCenter().Background(dotColor);
                            
                            if (!isLast)
                            {
                                dotCol.Item().Width(2).Height(30).Background(Colors.Grey.Lighten2);
                            }
                        });
                        
                        row.RelativeItem().Padding(10).Column(contentCol =>
                        {
                            var statusName = evt.Status?.Name ?? evt.EventType ?? "Status Update";
                            contentCol.Item().Text(statusName).Bold().FontSize(10).FontColor(isFirst ? Colors.Green.Darken2 : Colors.Grey.Darken3);
                            
                            if (!string.IsNullOrEmpty(evt.Remarks))
                            {
                                contentCol.Item().Height(3);
                                contentCol.Item().Text(evt.Remarks).FontSize(9).FontColor(Colors.Grey.Darken1);
                            }
                            
                            if (!string.IsNullOrEmpty(evt.LocationName))
                            {
                                contentCol.Item().Height(3);
                                contentCol.Item().Text($"Location: {evt.LocationName}").FontSize(8).FontColor(Colors.Green.Darken1);
                            }
                        });
                    });
                    
                    if (!isLast)
                    {
                        timelineCol.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten3);
                    }
                }
            });
        });
    }
}
