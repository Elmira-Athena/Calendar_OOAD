using Calendar.Core.Entities;

namespace Calendar.Core.Interfaces;

public interface IReminderRepository
{
    Task<List<Reminder>> GetByAppointmentAsync(Guid appointmentId, CancellationToken ct = default);
    Task<List<Reminder>> GetDueRemindersAsync(Guid userId, CancellationToken ct = default);
    Task<Reminder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(Reminder reminder);
    void Remove(Reminder reminder);
    Task SaveChangesAsync(CancellationToken ct = default);
}
