using Microsoft.EntityFrameworkCore;
using Net4Courier.Infrastructure.Data;
using Net4Courier.Kernel.Enums;

namespace Net4Courier.Web.Services;

public class GlobalSearchService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public GlobalSearchService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<SearchResult>> SearchAsync(string query, int maxResults = 15)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<SearchResult>();

        var results = new List<SearchResult>();
        query = query.Trim().ToLower();

        await using var context = await _contextFactory.CreateDbContextAsync();

        var awbResults = await SearchAWBsAsync(context, query, maxResults / 3 + 2);
        var customerResults = await SearchCustomersAsync(context, query, maxResults / 3 + 2);
        var invoiceResults = await SearchInvoicesAsync(context, query, maxResults / 3 + 2);

        results.AddRange(awbResults);
        results.AddRange(customerResults);
        results.AddRange(invoiceResults);

        return results.Take(maxResults).ToList();
    }

    private async Task<List<SearchResult>> SearchAWBsAsync(ApplicationDbContext context, string query, int limit)
    {
        var awbs = await context.InscanMasters
            .Where(a => !a.IsDeleted && 
                (a.AWBNo.ToLower().Contains(query) ||
                 (a.Consignor != null && a.Consignor.ToLower().Contains(query)) ||
                 (a.Consignee != null && a.Consignee.ToLower().Contains(query))))
            .OrderByDescending(a => a.TransactionDate)
            .Take(limit)
            .Select(a => new { a.AWBNo, a.Consignor, a.ConsigneeCity, a.ConsigneeCountry, a.CourierStatusId })
            .ToListAsync();

        return awbs.Select(a => new SearchResult
        {
            Type = SearchResultType.AWB,
            Title = a.AWBNo,
            Subtitle = $"{a.Consignor} → {a.ConsigneeCity}, {a.ConsigneeCountry}",
            Status = a.CourierStatusId.ToString(),
            NavigateUrl = $"/tracking/{a.AWBNo}",
            Icon = "LocalShipping"
        }).ToList();
    }

    private async Task<List<SearchResult>> SearchCustomersAsync(ApplicationDbContext context, string query, int limit)
    {
        var customers = await context.Parties
            .Where(p => !p.IsDeleted && p.IsActive &&
                p.PartyType == Net4Courier.Masters.Entities.PartyType.Customer &&
                (p.Name.ToLower().Contains(query) ||
                 (p.Code != null && p.Code.ToLower().Contains(query))))
            .OrderBy(p => p.Name)
            .Take(limit)
            .Select(p => new { p.Id, p.Name, p.Code, p.City })
            .ToListAsync();

        return customers.Select(c => new SearchResult
        {
            Type = SearchResultType.Customer,
            Title = c.Name,
            Subtitle = $"{c.Code} - {c.City}",
            Status = null,
            NavigateUrl = $"/customer-master/{c.Id}",
            Icon = "Person"
        }).ToList();
    }

    private async Task<List<SearchResult>> SearchInvoicesAsync(ApplicationDbContext context, string query, int limit)
    {
        var invoices = await context.Invoices
            .Include(i => i.Party)
            .Where(i => !i.IsDeleted &&
                (i.InvoiceNo.ToLower().Contains(query) ||
                 (i.Party != null && i.Party.Name.ToLower().Contains(query))))
            .OrderByDescending(i => i.InvoiceDate)
            .Take(limit)
            .Select(i => new { i.Id, i.InvoiceNo, PartyName = i.Party != null ? i.Party.Name : "", i.TotalAmount, i.CurrencyCode })
            .ToListAsync();

        return invoices.Select(i => new SearchResult
        {
            Type = SearchResultType.Invoice,
            Title = i.InvoiceNo,
            Subtitle = $"{i.PartyName} - {i.CurrencyCode} {i.TotalAmount:N2}",
            Status = null,
            NavigateUrl = $"/invoice-view/{i.Id}",
            Icon = "Receipt"
        }).ToList();
    }
}

public class SearchResult
{
    public SearchResultType Type { get; set; }
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string? Status { get; set; }
    public string NavigateUrl { get; set; } = "";
    public string Icon { get; set; } = "";
}

public enum SearchResultType
{
    AWB,
    Customer,
    Invoice
}
