using Calendar.Core.Entities;
using Calendar.Core.Interfaces;
using Calendar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly CalendarDbContext _context;

    public AppointmentRepository(CalendarDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Appointments
            .Include(x => x.Reminders)
            .Include(x => x.RecurrenceRule)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    /// <summary>
    /// Returns personal appointments for the user (not GroupMeetings).
    /// GroupMeetings are fetched separately via IGroupMeetingRepository.
    /// </summary>
    public async Task<List<Appointment>> GetByDateRangeAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _context.Appointments
            .Where(x => EF.Property<string>(x, "Discriminator") == "Appointment") // only personal appointments
            .Include(x => x.Reminders)
            .Include(x => x.RecurrenceRule)
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.StartTime >= from && x.StartTime <= to)
            .OrderBy(x => x.StartTime)
            .ToListAsync(ct);
    }

    public async Task<List<Appointment>> GetConflictingAsync(Guid userId, DateTime start, DateTime end, Guid? excludeId = null, CancellationToken ct = default)
    {
        // Conflicts include:
        // 1. Personal appointments for this user
        // 2. GroupMeetings where this user is a participant
        var query = _context.Appointments
            .AsNoTracking()
            .Where(x => x.StartTime < end && x.EndTime > start)
            .Where(x => 
                (EF.Property<string>(x, "Discriminator") == "Appointment" && x.UserId == userId) ||
                (EF.Property<string>(x, "Discriminator") == "GroupMeeting" && 
                 _context.Participants.Any(p => p.MeetingId == x.Id && p.UserId == userId))
            );

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<List<Appointment>> GetRecurringParentsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Appointments
            .Include(x => x.Reminders)
            .Include(x => x.RecurrenceRule)
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.RecurrenceRuleId != null)
            .ToListAsync(ct);
    }

    public void Add(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
    }

    public void Update(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
