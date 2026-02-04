using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Net4Courier.Operations.Entities;
using Net4Courier.Kernel.Enums;
using Microsoft.AspNetCore.Hosting;

namespace Net4Courier.Web.Services;

public class AWBPrintService
{
    private readonly byte[]? _logoData;

    public AWBPrintService(IWebHostEnvironment? env = null)
    {
        if (env != null)
        {
            var logoPath = Path.Combine(env.WebRootPath, "images", "gatex-logo.png");
            if (File.Exists(logoPath))
            {
                _logoData = File.ReadAllBytes(logoPath);
            }
        }
    }

    public byte[] GenerateA5AWB(InscanMaster shipment, string? companyName = null)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(10);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));
                
                page.Content().Column(column =>
                {
                    column.Spacing(3);
                    
                    BuildHeader(column, shipment, companyName ?? "Net4Courier");
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

    private void BuildHeader(ColumnDescriptor column, InscanMaster shipment, string companyName)
    {
        column.Item().Border(1).Row(row =>
        {
            row.RelativeItem(2).Padding(5).Column(headerCol =>
            {
                if (_logoData != null)
                {
                    headerCol.Item().Height(35).Image(_logoData).FitHeight();
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
                orgCol.Item().AlignCenter().Text(shipment.OriginPortCode ?? "---").Bold().FontSize(14);
            });
            
            row.RelativeItem(1).BorderLeft(1).Padding(3).Column(destCol =>
            {
                destCol.Item().Text("DEST").FontSize(6);
                destCol.Item().AlignCenter().Text(shipment.DestinationPortCode ?? "---").Bold().FontSize(14);
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
                    rightCol.Item().Text(shipment.ConsignorPhone ?? shipment.ConsignorMobile ?? "").FontSize(8);
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
                    rightCol.Item().Text(shipment.ConsigneePhone ?? shipment.ConsigneeMobile ?? "").FontSize(8);
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
                    c.Item().Text("SVC CODE").FontSize(6);
                    c.Item().AlignCenter().Text("---").FontSize(10);
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

    public byte[] GenerateLabel(InscanMaster shipment, string? companyName = null)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(100, 150, Unit.Millimetre);
                page.Margin(2);
                page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Arial"));
                
                page.Content().Border(1).Row(mainRow =>
                {
                    mainRow.RelativeItem(3).Column(column =>
                    {
                        column.Spacing(0);
                        
                        BuildGateexHeader(column, shipment, companyName ?? "Net4Courier");
                        BuildGateexOriginDest(column, shipment);
                        BuildGateexService(column, shipment);
                        BuildGateexWeight(column, shipment);
                        BuildGateexShipper(column, shipment);
                        BuildGateexConsignee(column, shipment);
                        BuildGateexRouteRemarks(column, shipment);
                        BuildGateexDescription(column, shipment);
                        BuildGateexReferences(column, shipment);
                    });
                    
                    mainRow.ConstantItem(28).BorderLeft(1).Column(rightCol =>
                    {
                        BuildGateexRightPanel(rightCol, shipment);
                    });
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

    private void BuildGateexHeader(ColumnDescriptor column, InscanMaster shipment, string companyName)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem(1).Padding(2).Column(c =>
            {
                if (_logoData != null)
                {
                    c.Item().Height(18).Image(_logoData).FitHeight();
                }
                else
                {
                    c.Item().Text(companyName).Bold().FontSize(10).FontColor(Colors.Red.Darken2);
                }
            });
            
            row.RelativeItem(2).BorderLeft(1).Padding(2).Column(c =>
            {
                c.Item().Text("Origin:").FontSize(5).FontColor(Colors.Grey.Darken1);
                c.Item().Text(GetPortCode(shipment.OriginPortCode, shipment.ConsignorCity)).Bold().FontSize(18);
            });
            
            row.RelativeItem(3).BorderLeft(1).Padding(2).Column(c =>
            {
                if (shipment.BarcodeImage != null)
                {
                    c.Item().AlignCenter().Height(22).Image(shipment.BarcodeImage);
                }
                c.Item().AlignCenter().Text(shipment.AWBNo).Bold().FontSize(8);
            });
        });
    }

    private void BuildGateexOriginDest(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Row(row =>
        {
            row.RelativeItem(1).Padding(2).Column(c =>
            {
                c.Item().Text("Destination:").FontSize(5).FontColor(Colors.Grey.Darken1);
                c.Item().Text(GetPortCode(shipment.DestinationPortCode, shipment.ConsigneeCity)).Bold().FontSize(18);
            });
            
            row.RelativeItem(2).BorderLeft(1).Padding(2).Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.ConstantItem(25).Text("Date:").FontSize(5).FontColor(Colors.Grey.Darken1);
                    r.RelativeItem().Text(shipment.TransactionDate.ToString("MMM d, yyyy")).FontSize(6);
                });
                c.Item().Row(r =>
                {
                    r.ConstantItem(35).Text("Foreign Ref:").FontSize(5).FontColor(Colors.Grey.Darken1);
                    r.RelativeItem().Text(shipment.ReferenceNo ?? "-").FontSize(5);
                });
                c.Item().Row(r =>
                {
                    r.ConstantItem(25).Text("Ref1:").FontSize(5).FontColor(Colors.Grey.Darken1);
                    r.RelativeItem().Text(shipment.AWBNo ?? "").FontSize(5);
                });
            });
            
            row.RelativeItem(1).BorderLeft(1).Padding(2).AlignCenter().Column(c =>
            {
                if (_logoData != null)
                {
                    c.Item().Height(20).Image(_logoData).FitHeight();
                }
            });
        });
    }

    private void BuildGateexService(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Row(row =>
        {
            var isExport = shipment.MovementTypeId == MovementType.InternationalExport || shipment.MovementTypeId == MovementType.InternationalImport;
            
            row.ConstantItem(28).BorderRight(1).Padding(1).AlignCenter().AlignMiddle().Text(isExport ? "EXP" : "DOM").Bold().FontSize(12);
            row.ConstantItem(28).BorderRight(1).Padding(1).AlignCenter().AlignMiddle().Text("PPX").Bold().FontSize(12);
            row.ConstantItem(18).BorderRight(1).Padding(1).AlignCenter().AlignMiddle().Text(shipment.PaymentModeId == PaymentMode.Prepaid ? "P" : "C").Bold().FontSize(12);
            row.RelativeItem().Padding(1).AlignCenter().AlignMiddle().Text((shipment.Pieces ?? 1).ToString()).Bold().FontSize(12);
        });
    }

    private void BuildGateexWeight(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Row(row =>
        {
            row.RelativeItem().Padding(2).Column(c =>
            {
                c.Item().Text($"Weight: {shipment.Weight?.ToString("F1") ?? "0.0"} KG").FontSize(6);
                c.Item().Text($"Chargeable: {shipment.ChargeableWeight?.ToString("F1") ?? "0.0"} KG").FontSize(6);
            });
            
            row.RelativeItem().BorderLeft(1).Padding(2).Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.ConstantItem(30).Text("Services:").FontSize(5).FontColor(Colors.Grey.Darken1);
                    if (shipment.CustomsValue > 0)
                    {
                        r.RelativeItem().Text($"INVOICE VALUE Customs: {shipment.CustomsValue?.ToString("F0")} {shipment.Currency ?? "AED"}").FontSize(5).FontColor(Colors.Red.Medium);
                    }
                });
            });
            
            row.ConstantItem(30).BorderLeft(1).Background(Colors.Grey.Lighten3).Padding(2).AlignCenter().Column(c =>
            {
                c.Item().Text("COD").Bold().FontSize(8);
                c.Item().Text(shipment.IsCOD ? (shipment.CODAmount?.ToString("F0") ?? "0") : "0").Bold().FontSize(10);
            });
        });
    }

    private void BuildGateexShipper(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Column(c =>
        {
            c.Item().Row(r =>
            {
                r.ConstantItem(35).Text("Account:").FontSize(5).FontColor(Colors.Grey.Darken1);
                r.RelativeItem().Text(shipment.CustomerId?.ToString() ?? "").FontSize(6);
            });
            c.Item().Text(shipment.Consignor ?? "").Bold().FontSize(7);
            c.Item().Text($"{shipment.ConsignorAddress1} {shipment.ConsignorAddress2}".Trim()).FontSize(6);
            c.Item().Row(r =>
            {
                r.RelativeItem().Text(shipment.ConsignorCity ?? "").FontSize(6);
                r.ConstantItem(40).Text(shipment.ConsignorPostalCode ?? "").FontSize(6);
            });
            c.Item().Row(r =>
            {
                r.RelativeItem().Text(shipment.ConsignorCountry ?? "").FontSize(6);
                r.RelativeItem().Text(shipment.ConsignorPhone ?? shipment.ConsignorMobile ?? "").FontSize(6);
            });
        });
    }

    private void BuildGateexConsignee(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(2).Padding(2).Column(c =>
        {
            c.Item().Text(shipment.Consignee ?? "").Bold().FontSize(8);
            c.Item().Text($"{shipment.ConsigneeAddress1} {shipment.ConsigneeAddress2}".Trim()).FontSize(6);
            c.Item().Height(2);
            c.Item().Text(shipment.ConsigneeCity ?? "").Bold().FontSize(8);
            c.Item().Text(shipment.ConsigneeCountry ?? "").FontSize(6);
            c.Item().Row(r =>
            {
                r.RelativeItem().Text(shipment.ConsigneePhone ?? shipment.ConsigneeMobile ?? "").Bold().FontSize(7);
                r.RelativeItem().Text(shipment.ConsigneeMobile ?? "").FontSize(6);
            });
        });
    }

    private void BuildGateexRouteRemarks(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Row(row =>
        {
            row.RelativeItem().Text($"Route: {shipment.DestinationPortCode ?? "N/A"}").FontSize(6);
        });
        
        column.Item().BorderTop(1).Padding(2).Row(row =>
        {
            row.RelativeItem().Text($"Remarks: {shipment.Remarks ?? ""}").FontSize(6);
        });
    }

    private void BuildGateexDescription(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Row(row =>
        {
            row.ConstantItem(45).Text("Description:").FontSize(5).FontColor(Colors.Grey.Darken1);
            row.RelativeItem().Text(shipment.CargoDescription ?? "Documents").FontSize(6);
        });
    }

    private void BuildGateexReferences(ColumnDescriptor column, InscanMaster shipment)
    {
        column.Item().BorderTop(1).Padding(2).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text($"Shipper Ref: {shipment.ReferenceNo ?? ""}").FontSize(5);
            });
            row.RelativeItem().Column(c =>
            {
                c.Item().Text($"Consignee Ref: {shipment.ReferenceNo ?? ""}").FontSize(5);
            });
        });
    }

    private void BuildGateexRightPanel(ColumnDescriptor column, InscanMaster shipment)
    {
        var currency = shipment.Currency ?? "AED";
        var hasDuty = (shipment.DutyVatAmount ?? 0) > 0;
        var hasCOD = shipment.IsCOD && (shipment.CODAmount ?? 0) > 0;
        var isDDP = hasDuty;
        
        column.Item().Padding(2).Column(c =>
        {
            c.Item().Text("PAYMENT MODE").FontSize(5).FontColor(Colors.Grey.Darken1);
            var paymentMode = isDDP ? "DDP" : "DDU";
            c.Item().Text(paymentMode).Bold().FontSize(8).FontColor(isDDP ? Colors.Red.Darken2 : Colors.Blue.Darken2);
        });
        
        column.Item().Height(2);
        
        if (hasDuty)
        {
            column.Item().Padding(2).Column(c =>
            {
                c.Item().Text("DUTY/VAT").FontSize(4).FontColor(Colors.Grey.Darken1);
                c.Item().Text("AMOUNT").FontSize(4).FontColor(Colors.Grey.Darken1);
                c.Item().Text($"{shipment.DutyVatAmount?.ToString("F0")} {currency}").FontSize(6);
            });
        }
        
        if (hasCOD)
        {
            column.Item().Padding(2).Column(c =>
            {
                c.Item().Text("COD COLLECT").FontSize(4).FontColor(Colors.Grey.Darken1);
                c.Item().Text("AMOUNT").FontSize(4).FontColor(Colors.Grey.Darken1);
                c.Item().Text($"{shipment.CODAmount?.ToString("F0")} {currency}").FontSize(6);
            });
        }
        
        var totalCollect = (shipment.DutyVatAmount ?? 0) + (shipment.CODAmount ?? 0);
        if (totalCollect > 0)
        {
            column.Item().Height(2);
            column.Item().Padding(2).Column(c =>
            {
                c.Item().Text("TOTAL COLLECT").FontSize(4).FontColor(Colors.Grey.Darken1);
                c.Item().Text("AMOUNT").FontSize(4).FontColor(Colors.Grey.Darken1);
                c.Item().Text($"{totalCollect:F0} {currency}").Bold().FontSize(7);
            });
        }
        
        if (shipment.BarcodeImageVertical != null)
        {
            column.Item().Height(5);
            column.Item().AlignCenter().Padding(1).Image(shipment.BarcodeImageVertical).FitWidth();
        }
    }

    private void BuildLabelHeader(ColumnDescriptor column, InscanMaster shipment, string companyName)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem(1).Padding(3).Column(c =>
            {
                if (_logoData != null)
                {
                    c.Item().Height(25).Image(_logoData).FitHeight();
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
                r.RelativeItem().Text(shipment.ConsignorCountry ?? "").FontSize(7);
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
                r.RelativeItem().Text(shipment.ConsigneeCountry ?? "").FontSize(7);
            });
            c.Item().Row(r =>
            {
                r.RelativeItem().Text(shipment.ConsigneePhone ?? shipment.ConsigneeMobile ?? "").Bold().FontSize(8);
                r.RelativeItem().Text(shipment.ConsigneePhone ?? "").FontSize(7);
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
                c.Item().Text($"SHIPMENT FROM: {shipment.ConsignorCountry ?? "UAE"}").Bold().FontSize(10);
            });
            row.RelativeItem().Column(c =>
            {
                c.Item().Text($"SHIPMENT TO: {shipment.ConsigneeCountry ?? ""}").Bold().FontSize(10);
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
                    r.RelativeItem().Text(shipment.ConsignorCountry ?? "").FontSize(9);
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
                    r.RelativeItem().Text(shipment.ConsigneeCountry ?? "").FontSize(9);
                });
                consigneeCol.Item().Height(5);
                consigneeCol.Item().Row(r =>
                {
                    r.ConstantItem(80).Text("Tel./Fax No.:").FontSize(9);
                    r.RelativeItem().Text(shipment.ConsigneePhone ?? shipment.ConsigneeMobile ?? "").FontSize(9);
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
