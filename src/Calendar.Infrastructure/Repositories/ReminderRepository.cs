using Calendar.Core.Entities;
using Calendar.Core.Interfaces;
using Calendar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Repositories;

public class ReminderRepository : IReminderRepository
{
    private readonly CalendarDbContext _context;

    public ReminderRepository(CalendarDbContext context)
    {
        _context = context;
    }

    public async Task<List<Reminder>> GetByAppointmentAsync(Guid appointmentId, CancellationToken ct = default)
    {
        return await _context.Reminders
            .Where(x => x.AppointmentId == appointmentId)
            .ToListAsync(ct);
    }

    public async Task<List<Reminder>> GetDueRemindersAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        
        // Due reminders are:
        // 1. Not triggered
        // 2. (StartTime - MinutesBefore) <= Now
        // 3. User is owner OR User is participant in a GroupMeeting
        return await _context.Reminders
            .Include(r => r.Appointment)
            .Where(r => !r.IsTriggered)
            .Where(r => r.Appointment.StartTime.AddMinutes(-r.MinutesBefore) <= now)
            .Where(r => 
                (EF.Property<string>(r.Appointment, "Discriminator") == "Appointment" && r.Appointment.UserId == userId) ||
                (EF.Property<string>(r.Appointment, "Discriminator") == "GroupMeeting" && 
                 _context.Participants.Any(p => p.MeetingId == r.AppointmentId && p.UserId == userId))
            )
            .ToListAsync(ct);
    }

    public async Task<Reminder?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Reminders.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public void Add(Reminder reminder)
    {
        _context.Reminders.Add(reminder);
    }

    public void Remove(Reminder reminder)
    {
        _context.Reminders.Remove(reminder);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
