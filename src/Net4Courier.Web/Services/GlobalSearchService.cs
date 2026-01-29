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
        var searchPattern = $"%{query}%";
        var awbs = await context.InscanMasters
            .Where(a => !a.IsDeleted && 
                (EF.Functions.ILike(a.AWBNo, searchPattern) ||
                 (a.Consignor != null && EF.Functions.ILike(a.Consignor, searchPattern)) ||
                 (a.Consignee != null && EF.Functions.ILike(a.Consignee, searchPattern))))
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
        var searchPattern = $"%{query}%";
        var customers = await context.Parties
            .Where(p => !p.IsDeleted && p.IsActive &&
                p.PartyType == Net4Courier.Masters.Entities.PartyType.Customer &&
                (EF.Functions.ILike(p.Name, searchPattern) ||
                 (p.Code != null && EF.Functions.ILike(p.Code, searchPattern))))
            .OrderBy(p => p.Name)
            .Take(limit)
            .Select(p => new { 
                p.Id, 
                p.Name, 
                p.Code, 
                City = p.Addresses.Select(a => a.City).FirstOrDefault() 
            })
            .ToListAsync();

        return customers.Select(c => new SearchResult
        {
            Type = SearchResultType.Customer,
            Title = c.Name,
            Subtitle = $"{c.Code} - {c.City ?? ""}",
            Status = null,
            NavigateUrl = $"/customer-master?search={Uri.EscapeDataString(c.Code ?? c.Name)}",
            Icon = "Person"
        }).ToList();
    }

    private async Task<List<SearchResult>> SearchInvoicesAsync(ApplicationDbContext context, string query, int limit)
    {
        var searchPattern = $"%{query}%";
        var invoices = await context.Invoices
            .Where(i => !i.IsDeleted &&
                (EF.Functions.ILike(i.InvoiceNo, searchPattern) ||
                 (i.CustomerName != null && EF.Functions.ILike(i.CustomerName, searchPattern))))
            .OrderByDescending(i => i.InvoiceDate)
            .Take(limit)
            .Select(i => new { i.Id, i.InvoiceNo, i.CustomerName, i.NetTotal })
            .ToListAsync();

        return invoices.Select(i => new SearchResult
        {
            Type = SearchResultType.Invoice,
            Title = i.InvoiceNo,
            Subtitle = $"{i.CustomerName} - {(i.NetTotal ?? 0):N2}",
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
