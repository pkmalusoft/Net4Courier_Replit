using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Net4Courier.Operations.Entities;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class AWBPrintService
{
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
                headerCol.Item().Text(companyName).Bold().FontSize(16).FontColor(Colors.Red.Darken2);
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
                page.Margin(3);
                page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Arial"));
                
                page.Content().Border(1).Column(column =>
                {
                    column.Spacing(0);
                    
                    BuildLabelHeader(column, shipment, companyName ?? "Net4Courier");
                    BuildLabelOriginDest(column, shipment);
                    BuildLabelService(column, shipment);
                    BuildLabelWeight(column, shipment);
                    BuildLabelShipper(column, shipment);
                    BuildLabelConsignee(column, shipment);
                    BuildLabelRoute(column, shipment);
                    BuildLabelDescription(column, shipment);
                    BuildLabelReferences(column, shipment);
                });
            });
        });
        
        return document.GeneratePdf();
    }

    private void BuildLabelHeader(ColumnDescriptor column, InscanMaster shipment, string companyName)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem(1).Padding(3).Column(c =>
            {
                c.Item().Text(companyName).Bold().FontSize(12).FontColor(Colors.Red.Darken2);
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
}
