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

    public byte[] GenerateA5AWB(InscanMaster shipment, string? companyName = null, byte[]? logoData = null)
    {
        var effectiveLogo = logoData ?? _logoData;
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(8);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));
                
                page.Content().Column(column =>
                {
                    column.Spacing(3);
                    
                    BuildHeader(column, shipment, companyName ?? "Net4Courier", effectiveLogo);
                    BuildShipperSection(column, shipment);
                    BuildReceiverSection(column, shipment);
                    BuildShipmentInfoSection(column, shipment);
                    BuildChargesSection(column, shipment);
                    BuildSignatureSection(column, shipment);
                });
            });
        });
        
        return document.GeneratePdf();
    }

    private void BuildHeader(ColumnDescriptor column, InscanMaster shipment, string companyName, byte[]? logoData = null)
    {
        var effectiveLogo = logoData ?? _logoData;
        column.Item().Border(1).Row(row =>
        {
            row.RelativeItem(2).Padding(5).Column(headerCol =>
            {
                if (effectiveLogo != null)
                {
                    headerCol.Item().Height(35).Image(effectiveLogo).FitHeight();
                }
                else
                {
                    headerCol.Item().Text(companyName).Bold().FontSize(16).FontColor(Colors.Red.Darken2);
                }
                headerCol.Item().Text("AIRWAYBILL").FontSize(10).Bold();
            });
            
            row.RelativeItem(1).BorderLeft(1).Padding(3).Column(orgCol =>
            {
                orgCol.Item().Text("ORG. STN").FontSize(6);
                var orgCode = GetCountryDisplayCode(shipment.OriginPortCode, shipment.ConsignorCountry);
                orgCol.Item().AlignCenter().Text(orgCode).Bold().FontSize(orgCode.Length > 5 ? 9 : 14);
            });
            
            row.RelativeItem(1).BorderLeft(1).Padding(3).Column(destCol =>
            {
                destCol.Item().Text("DEST").FontSize(6);
                var destCode = GetCountryDisplayCode(shipment.DestinationPortCode, shipment.ConsigneeCountry);
                destCol.Item().AlignCenter().Text(destCode).Bold().FontSize(destCode.Length > 5 ? 9 : 14);
            });
            
            row.RelativeItem(3).BorderLeft(1).Padding(3).Column(barcodeCol =>
            {
                if (shipment.BarcodeImage != null)
                {
                    barcodeCol.Item().AlignCenter().Height(40).Image(shipment.BarcodeImage);
                }
                barcodeCol.Item().AlignCenter().Text($"*{shipment.AWBNo}*").FontSize(8);
            });
        });
    }

    private void BuildShipperSection(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().Border(1).Column(section =>
        {
            section.Item().Background(Colors.Red.Lighten4).Padding(2).Text("1 FROM (SHIPPER)").Bold().FontSize(7);
            
            section.Item().Padding(3).Row(row =>
            {
                row.RelativeItem(3).Column(leftCol =>
                {
                    leftCol.Item().Row(nameRow =>
                    {
                        nameRow.RelativeItem().Text("Shipper's Account No:").FontSize(6);
                        nameRow.RelativeItem().Text(shipment.CustomerId?.ToString() ?? "").FontSize(7);
                    });
                    leftCol.Item().Row(fromRow =>
                    {
                        fromRow.RelativeItem().Text("FROM (Your Name): Print Please").FontSize(6);
                    });
                    leftCol.Item().Text(shipment.Consignor ?? "").Bold().FontSize(9);
                    
                    leftCol.Item().Height(3);
                    leftCol.Item().Text("Company").FontSize(6);
                    leftCol.Item().Text(shipment.Consignor ?? "").FontSize(8);
                    
                    leftCol.Item().Height(3);
                    leftCol.Item().Text("Street Address").FontSize(6);
                    leftCol.Item().Text($"{shipment.ConsignorAddress1} {shipment.ConsignorAddress2}".Trim()).FontSize(8);
                });
                
                row.RelativeItem(1).BorderLeft(1).PaddingLeft(3).Column(rightCol =>
                {
                    rightCol.Item().Text("Phone Number:").FontSize(6);
                    rightCol.Item().Text(CombinePhoneNumbers(shipment.ConsignorPhone, shipment.ConsignorMobile)).FontSize(8);
                });
            });
            
            section.Item().BorderTop(1).Padding(3).Row(row =>
            {
                row.RelativeItem(2).Column(cityCol =>
                {
                    cityCol.Item().Text("City").FontSize(6);
                    cityCol.Item().Text(shipment.ConsignorCity ?? "").Bold().FontSize(9);
                });
                row.RelativeItem(2).BorderLeft(1).PaddingLeft(3).Column(stateCol =>
                {
                    stateCol.Item().Text("State/Province").FontSize(6);
                    stateCol.Item().Text(shipment.ConsignorState ?? "").FontSize(8);
                });
                row.RelativeItem(1).BorderLeft(1).PaddingLeft(3).Column(countryCol =>
                {
                    countryCol.Item().Text("Country").FontSize(6);
                    countryCol.Item().Text(shipment.ConsignorCountry ?? "").FontSize(8);
                });
                row.RelativeItem(1).BorderLeft(1).PaddingLeft(3).Column(zipCol =>
                {
                    zipCol.Item().Text("ZIP/Postal Code").FontSize(6);
                    zipCol.Item().Text(shipment.ConsignorPostalCode ?? "").FontSize(8);
                });
            });
        });
    }

    private void BuildReceiverSection(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().Border(1).Column(section =>
        {
            section.Item().Background(Colors.Red.Lighten4).Padding(2).Text("2 TO (RECEIVER)").Bold().FontSize(7);
            
            section.Item().Padding(3).Row(row =>
            {
                row.RelativeItem(3).Column(leftCol =>
                {
                    leftCol.Item().Row(toRow =>
                    {
                        toRow.RelativeItem().Text("To (Receiver Name): Print Please").FontSize(6);
                    });
                    leftCol.Item().Text(shipment.Consignee ?? "").Bold().FontSize(9);
                    
                    leftCol.Item().Height(3);
                    leftCol.Item().Text("Company").FontSize(6);
                    leftCol.Item().Text(shipment.Consignee ?? "").FontSize(8);
                    
                    leftCol.Item().Height(3);
                    leftCol.Item().Text("Street Address (ARAMEX CANNOT DELIVER TO A P.O. BOX)").FontSize(6);
                    leftCol.Item().Text($"{shipment.ConsigneeAddress1} {shipment.ConsigneeAddress2}".Trim()).FontSize(8);
                });
                
                row.RelativeItem(1).BorderLeft(1).PaddingLeft(3).Column(rightCol =>
                {
                    rightCol.Item().Text("Phone Number:").FontSize(6);
                    rightCol.Item().Text(CombinePhoneNumbers(shipment.ConsigneePhone, shipment.ConsigneeMobile)).FontSize(8);
                });
            });
            
            section.Item().BorderTop(1).Padding(3).Row(row =>
            {
                row.RelativeItem(2).Column(cityCol =>
                {
                    cityCol.Item().Text("City").FontSize(6);
                    cityCol.Item().Text(shipment.ConsigneeCity ?? "").Bold().FontSize(9);
                });
                row.RelativeItem(2).BorderLeft(1).PaddingLeft(3).Column(stateCol =>
                {
                    stateCol.Item().Text("State/Province").FontSize(6);
                    stateCol.Item().Text(shipment.ConsigneeState ?? "").FontSize(8);
                });
                row.RelativeItem(1).BorderLeft(1).PaddingLeft(3).Column(countryCol =>
                {
                    countryCol.Item().Text("Country").FontSize(6);
                    countryCol.Item().Text(shipment.ConsigneeCountry ?? "").FontSize(8);
                });
                row.RelativeItem(1).BorderLeft(1).PaddingLeft(3).Column(zipCol =>
                {
                    zipCol.Item().Text("ZIP/Postal Code").FontSize(6);
                    zipCol.Item().Text(shipment.ConsigneePostalCode ?? "").FontSize(8);
                });
            });
        });
    }

    private void BuildShipmentInfoSection(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().Border(1).Row(row =>
        {
            row.RelativeItem(1).BorderRight(1).Column(infoCol =>
            {
                infoCol.Item().Background(Colors.Red.Lighten4).Padding(2).Text("4 SHIPMENT INFORMATION").Bold().FontSize(7);
                
                infoCol.Item().Padding(3).Row(infoRow =>
                {
                    infoRow.RelativeItem().Column(c =>
                    {
                        c.Item().Text("No. of Pieces").FontSize(6);
                        c.Item().AlignCenter().Text(shipment.Pieces?.ToString() ?? "1").Bold().FontSize(12);
                    });
                    infoRow.RelativeItem().BorderLeft(1).PaddingLeft(3).Column(c =>
                    {
                        c.Item().Text("\"Actual\" Weight").FontSize(6);
                        c.Item().Row(wRow =>
                        {
                            wRow.RelativeItem().AlignCenter().Text(shipment.Weight?.ToString("F2") ?? "0.00").Bold().FontSize(12);
                            wRow.ConstantItem(20).Text("KG").FontSize(8);
                        });
                    });
                    infoRow.RelativeItem().BorderLeft(1).PaddingLeft(3).Column(c =>
                    {
                        c.Item().Text("\"Chargeable\" Weight").FontSize(6);
                        c.Item().Row(wRow =>
                        {
                            wRow.RelativeItem().AlignCenter().Text(shipment.ChargeableWeight?.ToString("F2") ?? "0.00").Bold().FontSize(12);
                            wRow.ConstantItem(20).Text("KG").FontSize(8);
                        });
                    });
                });
                
                infoCol.Item().BorderTop(1).Padding(3).Column(descCol =>
                {
                    descCol.Item().Text("Description of Goods/Harmonized Code").FontSize(6);
                    descCol.Item().Text(shipment.CargoDescription ?? "Documents").FontSize(8);
                });
                
                infoCol.Item().BorderTop(1).Padding(3).Row(valRow =>
                {
                    valRow.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Customs Value").FontSize(6);
                        c.Item().Text(shipment.CustomsValue?.ToString("F2") ?? "0.00").FontSize(8);
                    });
                    valRow.RelativeItem().BorderLeft(1).PaddingLeft(3).Column(c =>
                    {
                        c.Item().Text("Currency").FontSize(6);
                        c.Item().Text(shipment.Currency ?? "AED").FontSize(8);
                    });
                    valRow.RelativeItem().BorderLeft(1).PaddingLeft(3).Column(c =>
                    {
                        c.Item().Text("Reference No.").FontSize(6);
                        c.Item().Text(shipment.ReferenceNo ?? "").Bold().FontSize(8);
                    });
                });
            });
            
            row.RelativeItem(1).Column(serviceCol =>
            {
                serviceCol.Item().Background(Colors.Red.Lighten4).Padding(2).Text("5 SERVICES").Bold().FontSize(7);
                
                serviceCol.Item().Padding(3).Row(svcRow =>
                {
                    svcRow.RelativeItem().Column(c =>
                    {
                        c.Item().Text("PROD GRP").FontSize(6);
                        c.Item().AlignCenter().Text(shipment.MovementTypeId == MovementType.InternationalExport || shipment.MovementTypeId == MovementType.InternationalImport ? "EXP" : "DOM").Bold().FontSize(14);
                    });
                    svcRow.RelativeItem().BorderLeft(1).PaddingLeft(3).Column(c =>
                    {
                        c.Item().Text("PROD TYP").FontSize(6);
                        c.Item().AlignCenter().Text("PPX").Bold().FontSize(14);
                    });
                });
                
                serviceCol.Item().BorderTop(1).Padding(3).Column(c =>
                {
                    var dutyVat = shipment.DutyVatAmount ?? 0;
                    var cod = shipment.CODAmount ?? 0;
                    var total = dutyVat + cod;
                    c.Item().Text($"DUTY /VAT : {(dutyVat > 0 ? dutyVat.ToString("N2") : "")}").FontSize(7).FontColor(Colors.Red.Darken2);
                    c.Item().Text($"COD : {(cod > 0 ? cod.ToString("N2") : "")}").FontSize(7).FontColor(Colors.Red.Darken2);
                    c.Item().Text($"TOTAL AMOUNT: {(total > 0 ? total.ToString("N2") : "")}").Bold().FontSize(7).FontColor(Colors.Red.Darken2);
                });
                
                serviceCol.Item().BorderTop(1).Padding(3).Column(c =>
                {
                    c.Item().Text("DOMESTIC ROUTING").FontSize(6);
                    c.Item().Text(shipment.ConsigneeCity ?? "").FontSize(8);
                });
            });
        });
    }

    private void BuildChargesSection(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().Border(1).Row(row =>
        {
            row.RelativeItem(1).BorderRight(1).Column(chargeCol =>
            {
                chargeCol.Item().Background(Colors.Red.Lighten4).Padding(2).Text("6 TRANSPORTATION CHARGES").Bold().FontSize(7);
                
                chargeCol.Item().Padding(3).Column(c =>
                {
                    c.Item().Text("Default to Shipper Account if Not Noted").FontSize(6);
                    c.Item().Height(3);
                    
                    c.Item().Row(r =>
                    {
                        r.ConstantItem(60).Text("Bill Shipper").FontSize(7);
                        r.ConstantItem(10).Border(0.5f).Height(8).Width(8);
                        r.ConstantItem(40).PaddingLeft(5).Text("Cash").FontSize(7);
                        r.ConstantItem(10).Border(0.5f).Height(8).Width(8);
                    });
                    c.Item().Height(2);
                    c.Item().Row(r =>
                    {
                        r.ConstantItem(60).Text("Prepaid Stock").FontSize(7);
                        r.ConstantItem(10).Border(0.5f).Height(8).Width(8);
                    });
                    c.Item().Height(2);
                    c.Item().Row(r =>
                    {
                        r.ConstantItem(60).Text("Bill Receiver Account (Collect)").FontSize(7);
                    });
                });
            });
            
            row.RelativeItem(1).Column(taxCol =>
            {
                taxCol.Item().Background(Colors.Red.Lighten4).Padding(2).Text("7 DUTIES AND TAXES").Bold().FontSize(7);
                
                taxCol.Item().Padding(3).Column(c =>
                {
                    c.Item().Text("Default to Receiver if Not Noted").FontSize(6);
                    c.Item().Height(3);
                    
                    c.Item().Row(r =>
                    {
                        r.ConstantItem(80).Text("Bill Shipper Account (Free Domicile)").FontSize(7);
                        r.ConstantItem(10).Border(0.5f).Height(8).Width(8);
                    });
                    c.Item().Height(2);
                    c.Item().Row(r =>
                    {
                        r.ConstantItem(80).Text("Bill Receiver").FontSize(7);
                        r.ConstantItem(10).Border(0.5f).Height(8).Width(8);
                    });
                });
            });
        });
        
        if (shipment.IsCOD)
        {
            column.Item().Border(1).Background(Colors.Yellow.Lighten4).Padding(3).Row(row =>
            {
                row.RelativeItem().Text("COD Amount:").Bold().FontSize(9);
                row.RelativeItem().Text($"{shipment.Currency ?? "AED"} {shipment.CODAmount?.ToString("F2") ?? "0.00"}").Bold().FontSize(12);
            });
        }
    }

    private void BuildSignatureSection(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().Border(1).Row(row =>
        {
            row.RelativeItem(1).BorderRight(1).Column(shipperSig =>
            {
                shipperSig.Item().Background(Colors.Red.Lighten4).Padding(2).Text("3 SHIPPER'S SIGNATURE & AUTHORIZATION").Bold().FontSize(7);
                
                shipperSig.Item().Padding(3).Column(c =>
                {
                    c.Item().Text("(Please sign) All pickup destination info/weights apply to this shipment and its Terms & Conditions. The Shipper Confirms that I have received").FontSize(5);
                    c.Item().Height(5);
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Shipper:").FontSize(7);
                        r.RelativeItem(2).BorderBottom(0.5f).Text(shipment.Consignor ?? "").FontSize(8);
                    });
                    c.Item().Height(5);
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Column(dateCol =>
                        {
                            dateCol.Item().Text("Date:").FontSize(6);
                            dateCol.Item().Text(shipment.TransactionDate.ToString("MM/dd/yyyy")).FontSize(8);
                        });
                        r.RelativeItem().Column(timeCol =>
                        {
                            timeCol.Item().Text("Time:").FontSize(6);
                            timeCol.Item().Text(shipment.CreatedAt.ToString("HH:mm")).FontSize(8);
                        });
                    });
                });
            });
            
            row.RelativeItem(1).Column(receiverSig =>
            {
                receiverSig.Item().Background(Colors.Red.Lighten4).Padding(2).Text("9 RECEIVER SIGNATURE").Bold().FontSize(7);
                
                receiverSig.Item().Padding(3).Column(c =>
                {
                    c.Item().Text("Received the above shipment in good order and condition").FontSize(6);
                    c.Item().Height(5);
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Receiver's Name:").FontSize(7);
                        r.RelativeItem(2).BorderBottom(0.5f).Text(shipment.DeliveredTo ?? "").FontSize(8);
                    });
                    c.Item().Height(5);
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Signature (Required):").FontSize(6);
                        r.RelativeItem(2).BorderBottom(0.5f).Height(15);
                    });
                });
            });
        });
    }

    public byte[] GenerateBulkA5AWB(List<InscanMaster> shipments, string? companyName = null, byte[]? logoData = null)
    {
        var effectiveLogo = logoData ?? _logoData;
        var document = Document.Create(container =>
        {
            foreach (var shipment in shipments)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(8);
                    page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));
                    
                    page.Content().Column(column =>
                    {
                        column.Spacing(3);
                        
                        BuildHeader(column, shipment, companyName ?? "Net4Courier", effectiveLogo);
                        BuildShipperSection(column, shipment);
                        BuildReceiverSection(column, shipment);
                        BuildShipmentInfoSection(column, shipment);
                        BuildChargesSection(column, shipment);
                        BuildSignatureSection(column, shipment);
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
                    page.Margin(2);
                    page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Arial"));

                    page.Content().Border(1).Column(column =>
                    {
                        column.Spacing(0);

                        LabelHeader(column, shipment, companyName ?? "Net4Courier", effectiveLogo);
                        LabelDestination(column, shipment);
                        LabelServiceCodes(column, shipment);
                        LabelWeightCharges(column, shipment);
                        LabelServiceAccount(column, shipment);
                        LabelShipper(column, shipment);
                        LabelConsignee(column, shipment);
                        LabelRemarks(column, shipment);
                        LabelDescription(column, shipment);
                        LabelReferences(column, shipment);
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
                page.Margin(2);
                page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Arial"));

                page.Content().Border(1).Column(column =>
                {
                    column.Spacing(0);

                    LabelHeader(column, shipment, companyName ?? "Net4Courier", effectiveLogo);
                    LabelDestination(column, shipment);
                    LabelServiceCodes(column, shipment);
                    LabelWeightCharges(column, shipment);
                    LabelServiceAccount(column, shipment);
                    LabelShipper(column, shipment);
                    LabelConsignee(column, shipment);
                    LabelRemarks(column, shipment);
                    LabelDescription(column, shipment);
                    LabelReferences(column, shipment);
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

    private void LabelHeader(ColumnDescriptor column, InscanMaster shipment, string companyName, byte[]? logoData)
    {
        var effectiveLogo = logoData ?? _logoData;
        column.Item().Row(row =>
        {
            row.RelativeItem(2).BorderRight(1).Padding(2).Column(c =>
            {
                if (effectiveLogo != null)
                {
                    c.Item().MaxHeight(20).Image(effectiveLogo).FitArea();
                }
                else
                {
                    c.Item().Text(companyName).Bold().FontSize(10);
                }
            });

            row.RelativeItem(2).BorderRight(1).Padding(2).Column(c =>
            {
                c.Item().Text("Origin").Bold().FontSize(6);
                c.Item().Text(GetPortCode(shipment.OriginPortCode, shipment.ConsignorCity)).Bold().FontSize(16);
            });

            row.RelativeItem(3).Padding(2).Column(c =>
            {
                if (shipment.BarcodeImage != null)
                {
                    c.Item().AlignCenter().MaxHeight(18).Image(shipment.BarcodeImage).FitArea();
                }
                c.Item().AlignCenter().Text(shipment.AWBNo).Bold().FontSize(8);
            });
        });
    }

    private void LabelDestination(ColumnDescriptor column, InscanMaster shipment)
    {
        var hasDuty = (shipment.DutyVatAmount ?? 0) > 0;
        var paymentMode = hasDuty ? "DDP" : "DDU";

        column.Item().BorderTop(1).Row(row =>
        {
            row.RelativeItem(2).BorderRight(1).Padding(2).Column(c =>
            {
                c.Item().Text("Destination").Bold().FontSize(6);
                c.Item().Text(GetPortCode(shipment.DestinationPortCode, shipment.ConsigneeCity)).Bold().FontSize(16);
            });

            row.RelativeItem(3).BorderRight(1).Padding(2).Column(c =>
            {
                c.Item().Text(text =>
                {
                    text.Span("Date : ").Bold().FontSize(6);
                    text.Span(shipment.TransactionDate.ToString("d MMM yyyy")).FontSize(6);
                });
                c.Item().Text(text =>
                {
                    text.Span("Foreign Ref: ").Bold().FontSize(6);
                    text.Span(shipment.ReferenceNo ?? "-").FontSize(6);
                });
                c.Item().Text(text =>
                {
                    text.Span("Ref: ").Bold().FontSize(6);
                    text.Span(shipment.AWBNo ?? "").FontSize(6);
                });
            });

            row.RelativeItem(2).Padding(2).AlignCenter().Column(c =>
            {
                c.Item().Text("Payment Mode").Bold().FontSize(6);
                c.Item().Text(paymentMode).Bold().FontSize(12);
            });
        });
    }

    private void LabelServiceCodes(ColumnDescriptor column, InscanMaster shipment)
    {
        var isExport = shipment.MovementTypeId == MovementType.InternationalExport || shipment.MovementTypeId == MovementType.InternationalImport;

        column.Item().BorderTop(1).Row(row =>
        {
            row.ConstantItem(25).BorderRight(1).Padding(1).AlignCenter().AlignMiddle().Text(isExport ? "EXP" : "DOM").Bold().FontSize(9);
            row.ConstantItem(25).BorderRight(1).Padding(1).AlignCenter().AlignMiddle().Text("PPX").Bold().FontSize(9);
            row.ConstantItem(15).BorderRight(1).Padding(1).AlignCenter().AlignMiddle().Text(shipment.PaymentModeId == PaymentMode.Prepaid ? "P" : "C").Bold().FontSize(9);
            row.ConstantItem(15).BorderRight(1).Padding(1).AlignCenter().AlignMiddle().Text((shipment.Pieces ?? 1).ToString()).Bold().FontSize(9);
            row.RelativeItem().Padding(1).Text("").FontSize(6);
        });
    }

    private void LabelWeightCharges(ColumnDescriptor column, InscanMaster shipment)
    {
        var currency = shipment.Currency ?? "AED";
        var codAmount = shipment.IsCOD ? (shipment.CODAmount ?? 0) : 0;
        var vatAmount = shipment.TaxAmount ?? 0;
        var dutyAmount = shipment.DutyVatAmount ?? 0;
        var totalAmount = codAmount + vatAmount + dutyAmount;

        column.Item().BorderTop(1).Row(row =>
        {
            row.RelativeItem(3).Padding(2).Column(c =>
            {
                c.Item().Text(text =>
                {
                    text.Span("Weight: ").Bold().FontSize(7);
                    text.Span($"{shipment.Weight?.ToString("F1") ?? "0.0"} kg").FontSize(7);
                });
                c.Item().Height(2);
                c.Item().Text(text =>
                {
                    text.Span("Chargeable Wt: ").Bold().FontSize(7);
                    text.Span($"{shipment.ChargeableWeight?.ToString("F1") ?? "0.0"} kg").FontSize(7);
                });
            });

            row.RelativeItem(2).BorderLeft(1).Padding(1).Column(c =>
            {
                c.Item().BorderBottom(0.5f).Padding(1).Row(r =>
                {
                    r.RelativeItem().Text("COD").Bold().FontSize(7);
                    r.RelativeItem().AlignRight().Text(codAmount > 0 ? codAmount.ToString("N2") : "").FontSize(7);
                });
                c.Item().BorderBottom(0.5f).Padding(1).Row(r =>
                {
                    r.RelativeItem().Text("VAT").Bold().FontSize(7);
                    r.RelativeItem().AlignRight().Text(vatAmount > 0 ? vatAmount.ToString("N2") : "").FontSize(7);
                });
                c.Item().BorderBottom(0.5f).Padding(1).Row(r =>
                {
                    r.RelativeItem().Text("DUTY").Bold().FontSize(7);
                    r.RelativeItem().AlignRight().Text(dutyAmount > 0 ? dutyAmount.ToString("N2") : "").FontSize(7);
                });
                c.Item().Padding(1).Row(r =>
                {
                    r.RelativeItem().Text($"TOTAL ({currency})").Bold().FontSize(7);
                    r.RelativeItem().AlignRight().Text(totalAmount > 0 ? totalAmount.ToString("N2") : "").Bold().FontSize(7);
                });
            });
        });
    }

    private void LabelServiceAccount(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Column(c =>
        {
            c.Item().Text(text =>
            {
                text.Span("Service Type:  ").Bold().FontSize(7);
                text.Span("Standard Express").FontSize(7);
            });
            c.Item().Text(text =>
            {
                text.Span("Account:").Bold().FontSize(7);
            });
        });
    }

    private void LabelShipper(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Column(c =>
        {
            c.Item().Text(Truncate(shipment.Consignor, 50)).Bold().FontSize(8);
        });
    }

    private void LabelConsignee(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Column(c =>
        {
            c.Item().Text(shipment.ConsigneeCountry ?? "").FontSize(7);
            c.Item().Text(Truncate(shipment.Consignee, 50)).Bold().FontSize(8);

            if (!string.IsNullOrWhiteSpace(shipment.ConsigneeAddress1))
                c.Item().Text($"FL: {shipment.ConsigneeAddress1}").FontSize(7);
            if (!string.IsNullOrWhiteSpace(shipment.ConsigneeAddress2))
                c.Item().Text(shipment.ConsigneeAddress2).FontSize(7);
            if (!string.IsNullOrWhiteSpace(shipment.ConsigneeLocation))
                c.Item().Text(shipment.ConsigneeLocation).FontSize(7);

            var phone = CombinePhoneNumbers(shipment.ConsigneePhone, shipment.ConsigneeMobile);
            if (!string.IsNullOrWhiteSpace(phone))
                c.Item().Text(phone).FontSize(7);
        });
    }

    private void LabelRemarks(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Row(row =>
        {
            row.ConstantItem(45).Text("Remarks:").Bold().FontSize(7);
            row.RelativeItem().Text(Truncate(shipment.Remarks, 60)).FontSize(7);
        });
    }

    private void LabelDescription(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Row(row =>
        {
            row.ConstantItem(45).Text("Description").Bold().FontSize(7);
            row.RelativeItem().Text(shipment.CargoDescription ?? "Documents").FontSize(7);
        });
    }

    private void LabelReferences(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Row(row =>
        {
            row.RelativeItem().Text("Shipper Ref").Bold().FontSize(6);
            row.RelativeItem().BorderLeft(1).PaddingLeft(4).Text("Consignee Ref:").Bold().FontSize(6);
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
