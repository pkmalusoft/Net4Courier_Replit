using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Masters.Entities;

namespace Net4Courier.Web.Services;

public interface ISLAPdfService
{
    Task<byte[]> GenerateSLAAgreementPdfAsync(long agreementId);
}

public class SLAPdfService : ISLAPdfService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public SLAPdfService(ApplicationDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<byte[]> GenerateSLAAgreementPdfAsync(long agreementId)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var agreement = await _dbContext.SLAAgreements
            .Include(s => s.Customer)
            .Include(s => s.Company)
            .FirstOrDefaultAsync(s => s.Id == agreementId && !s.IsDeleted);

        if (agreement == null)
            throw new ArgumentException("Agreement not found");

        var transitRules = await _dbContext.SLATransitRules
            .Include(r => r.ServiceType)
            .Include(r => r.OriginCountry)
            .Include(r => r.DestinationCountry)
            .Include(r => r.OriginCity)
            .Include(r => r.DestinationCity)
            .Where(r => r.SLAAgreementId == agreementId && !r.IsDeleted)
            .OrderBy(r => r.ServiceType != null ? r.ServiceType.Name : "")
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, agreement));
                page.Content().Element(c => ComposeContent(c, agreement, transitRules));
                page.Footer().Element(ComposeFooter);
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private void ComposeHeader(IContainer container, SLAAgreement agreement)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    if (agreement.Company != null)
                    {
                        col.Item().Text(agreement.Company.Name).FontSize(16).Bold();
                        if (!string.IsNullOrEmpty(agreement.Company.Address))
                            col.Item().Text(agreement.Company.Address).FontSize(9);
                        if (!string.IsNullOrEmpty(agreement.Company.Phone))
                            col.Item().Text($"Tel: {agreement.Company.Phone}").FontSize(9);
                        if (!string.IsNullOrEmpty(agreement.Company.Email))
                            col.Item().Text($"Email: {agreement.Company.Email}").FontSize(9);
                    }
                });

                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text("SERVICE LEVEL AGREEMENT").FontSize(12).Bold();
                    col.Item().Text($"Agreement No: {agreement.AgreementNo}").FontSize(10);
                    col.Item().Text($"Date: {agreement.AgreementDate:dd-MMM-yyyy}").FontSize(10);
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
            column.Item().PaddingTop(15);
        });
    }

    private void ComposeContent(IContainer container, SLAAgreement agreement, List<SLATransitRule> transitRules)
    {
        container.Column(column =>
        {
            column.Item().Text("Customer Information").FontSize(12).Bold();
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(120);
                    columns.RelativeColumn();
                    columns.ConstantColumn(120);
                    columns.RelativeColumn();
                });

                table.Cell().Text("Customer:").Bold();
                table.Cell().Text(agreement.Customer?.Name ?? "-");
                table.Cell().Text("Account Type:").Bold();
                table.Cell().Text(agreement.AccountType.ToString());

                table.Cell().Text("Contact:").Bold();
                table.Cell().Text(agreement.Customer?.ContactPerson ?? "-");
                table.Cell().Text("Status:").Bold();
                table.Cell().Text(agreement.Status.ToString());
            });

            column.Item().PaddingTop(15).Text("Agreement Details").FontSize(12).Bold();
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(120);
                    columns.RelativeColumn();
                    columns.ConstantColumn(120);
                    columns.RelativeColumn();
                });

                table.Cell().Text("Title:").Bold();
                table.Cell().ColumnSpan(3).Text(agreement.Title ?? "-");

                table.Cell().Text("Credit Limit:").Bold();
                table.Cell().Text($"{agreement.CreditLimit:N2}");
                table.Cell().Text("Payment Terms:").Bold();
                table.Cell().Text($"{agreement.PaymentTermsDays} days");

                table.Cell().Text("Agreement Date:").Bold();
                table.Cell().Text($"{agreement.AgreementDate:dd-MMM-yyyy}");
                table.Cell().Text("Expiry Date:").Bold();
                table.Cell().Text(agreement.ExpiryDate.HasValue ? $"{agreement.ExpiryDate:dd-MMM-yyyy}" : "No Expiry");

                table.Cell().Text("Liability Limit:").Bold();
                table.Cell().Text($"{agreement.LiabilityLimitUSD:N2} {agreement.LiabilityLimitCurrency}");
                table.Cell().Text("Max Package Weight:").Bold();
                table.Cell().Text($"{agreement.MaxPackageWeight:N2} kg");
            });

            column.Item().PaddingTop(15).Text("Volumetric Settings").FontSize(12).Bold();
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Domestic Divisor").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Air Divisor").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Road Divisor").Bold();

                table.Cell().Padding(5).Text($"{agreement.VolumetricDivisorDomestic:N0}");
                table.Cell().Padding(5).Text($"{agreement.VolumetricDivisorAir:N0}");
                table.Cell().Padding(5).Text($"{agreement.VolumetricDivisorRoad:N0}");
            });

            if (transitRules.Any())
            {
                column.Item().PaddingTop(15).Text("Transit Commitments").FontSize(12).Bold();
                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Service Type").Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Origin").Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Destination").Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Transit Days").Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Default").Bold();

                    foreach (var rule in transitRules)
                    {
                        table.Cell().Padding(5).Text(rule.ServiceType?.Name ?? "All");
                        var origin = rule.OriginCountry?.Name ?? rule.OriginCity?.Name ?? rule.OriginZone ?? "Any";
                        table.Cell().Padding(5).Text(origin);
                        var dest = rule.DestinationCountry?.Name ?? rule.DestinationCity?.Name ?? rule.DestinationZone ?? "Any";
                        table.Cell().Padding(5).Text(dest);
                        table.Cell().Padding(5).Text($"{rule.TransitDays}");
                        table.Cell().Padding(5).Text(rule.IsDefault ? "Yes" : "No");
                    }
                });
            }

            if (!string.IsNullOrEmpty(agreement.SpecialTerms))
            {
                column.Item().PaddingTop(15).Text("Special Terms").FontSize(12).Bold();
                column.Item().PaddingTop(5).Text(agreement.SpecialTerms);
            }

            if (!string.IsNullOrEmpty(agreement.Notes))
            {
                column.Item().PaddingTop(15).Text("Notes").FontSize(12).Bold();
                column.Item().PaddingTop(5).Text(agreement.Notes);
            }

            column.Item().PaddingTop(30).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().PaddingTop(40).LineHorizontal(1);
                    col.Item().Text("Authorized Signature (Company)").FontSize(9).Italic();
                });
                row.ConstantItem(50);
                row.RelativeItem().Column(col =>
                {
                    col.Item().PaddingTop(40).LineHorizontal(1);
                    col.Item().Text("Customer Signature").FontSize(9).Italic();
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Page ");
            text.CurrentPageNumber();
            text.Span(" of ");
            text.TotalPages();
        });
    }
}
