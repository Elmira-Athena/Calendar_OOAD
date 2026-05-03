using Calendar.Core.Entities;
using Calendar.Core.Interfaces;
using Calendar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Repositories;

public class RecurrenceRuleRepository : IRecurrenceRuleRepository
{
    private readonly CalendarDbContext _context;

    public RecurrenceRuleRepository(CalendarDbContext context)
    {
        _context = context;
    }

    public async Task<RecurrenceRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RecurrenceRules.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public void Add(RecurrenceRule rule)
    {
        _context.RecurrenceRules.Add(rule);
    }

    public void Remove(RecurrenceRule rule)
    {
        _context.RecurrenceRules.Remove(rule);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
