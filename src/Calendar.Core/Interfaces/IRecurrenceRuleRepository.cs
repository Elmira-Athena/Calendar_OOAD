using Calendar.Core.Entities;

namespace Calendar.Core.Interfaces;

public interface IRecurrenceRuleRepository
{
    Task<RecurrenceRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(RecurrenceRule rule);
    void Remove(RecurrenceRule rule);
    Task SaveChangesAsync(CancellationToken ct = default);
}
