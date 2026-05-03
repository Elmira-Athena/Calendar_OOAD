using Calendar.Core.Entities;

namespace Calendar.Core.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Appointment>> GetByDateRangeAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<List<Appointment>> GetConflictingAsync(Guid userId, DateTime start, DateTime end, Guid? excludeId = null, CancellationToken ct = default);
    Task<List<Appointment>> GetRecurringParentsAsync(Guid userId, CancellationToken ct = default);
    void Add(Appointment appointment);
    void Update(Appointment appointment);
    Task SaveChangesAsync(CancellationToken ct = default);
}
