using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Modules.GeneralLedger.Models;

namespace Server.Modules.GeneralLedger.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JournalEntriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public JournalEntriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JournalEntry>>> GetAll()
    {
        var entries = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Include(j => j.Branch)
            .Include(j => j.Department)
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync();
        return Ok(entries);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JournalEntry>> GetById(Guid id)
    {
        var entry = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Currency)
            .Include(j => j.Branch)
            .Include(j => j.Department)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (entry == null)
            return NotFound();

        return Ok(entry);
    }

    [HttpPost]
    public async Task<ActionResult<JournalEntry>> Create(JournalEntry entry)
    {
        if (entry.Lines == null || entry.Lines.Count == 0)
            return BadRequest("Journal entry must have at least one line");

        var totalDebit = entry.Lines.Sum(l => l.Debit);
        var totalCredit = entry.Lines.Sum(l => l.Credit);

        if (Math.Abs(totalDebit - totalCredit) > 0.01m)
            return BadRequest("Debits must equal credits");

        entry.EntryNumber = await GenerateEntryNumber();
        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(Guid id)
    {
        var entry = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (entry == null)
            return NotFound();

        if (entry.Status == JournalEntryStatus.Posted)
            return BadRequest("Entry is already posted");

        var totalDebit = entry.Lines.Sum(l => l.Debit);
        var totalCredit = entry.Lines.Sum(l => l.Credit);

        if (Math.Abs(totalDebit - totalCredit) > 0.01m)
            return BadRequest("Debits must equal credits");

        entry.Status = JournalEntryStatus.Posted;
        entry.PostedDate = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return Ok(entry);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, JournalEntry entry)
    {
        if (id != entry.Id)
            return BadRequest();

        var existing = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (existing == null)
            return NotFound();

        if (existing.Status == JournalEntryStatus.Posted)
            return BadRequest("Cannot modify posted entries");

        existing.EntryDate = entry.EntryDate;
        existing.Description = entry.Description;

        _context.JournalEntryLines.RemoveRange(existing.Lines);
        existing.Lines = entry.Lines;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entry = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (entry == null)
            return NotFound();

        if (entry.Status == JournalEntryStatus.Posted)
            return BadRequest("Cannot delete posted entries");

        _context.JournalEntries.Remove(entry);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string> GenerateEntryNumber()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"JE{year}";
        
        var lastEntry = await _context.JournalEntries
            .Where(j => j.EntryNumber.StartsWith(prefix))
            .OrderByDescending(j => j.EntryNumber)
            .FirstOrDefaultAsync();

        if (lastEntry == null)
            return $"{prefix}0001";

        var lastNumber = int.Parse(lastEntry.EntryNumber.Substring(prefix.Length));
        return $"{prefix}{(lastNumber + 1):D4}";
    }
}
