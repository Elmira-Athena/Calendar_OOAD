using Calendar.Core.Entities;
using Calendar.Core.Interfaces;
using Calendar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Infrastructure.Repositories;

public class GroupMeetingRepository : IGroupMeetingRepository
{
    private readonly CalendarDbContext _context;

    public GroupMeetingRepository(CalendarDbContext context)
    {
        _context = context;
    }

    public async Task<GroupMeeting?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Appointments
            .OfType<GroupMeeting>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<GroupMeeting?> GetByIdWithParticipantsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Appointments
            .OfType<GroupMeeting>()
            .Include(x => x.Reminders)
            .Include(x => x.Participants)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<List<GroupMeeting>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _context.Appointments
            .OfType<GroupMeeting>()
            .Include(x => x.Participants)
            .AsNoTracking()
            .Where(x => x.StartTime < to && x.EndTime > from)
            .OrderBy(x => x.StartTime)
            .ToListAsync(ct);
    }

    public async Task<GroupMeeting?> FindMatchingAsync(string name, DateTime start, DateTime end, CancellationToken ct = default)
    {
        return await _context.Appointments
            .OfType<GroupMeeting>()
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Name == name && x.StartTime == start && x.EndTime == end, ct);
    }

    public async Task<bool> HasParticipantAsync(Guid meetingId, Guid userId, CancellationToken ct = default)
    {
        return await _context.Participants
            .AnyAsync(x => x.MeetingId == meetingId && x.UserId == userId, ct);
    }

    public async Task<Participant?> GetParticipantAsync(Guid meetingId, Guid userId, CancellationToken ct = default)
    {
        return await _context.Participants
            .FirstOrDefaultAsync(x => x.MeetingId == meetingId && x.UserId == userId, ct);
    }

    public void Add(GroupMeeting meeting)
    {
        _context.Appointments.Add(meeting);
    }

    public void AddParticipant(Participant participant)
    {
        _context.Participants.Add(participant);
    }

    public void RemoveParticipant(Participant participant)
    {
        _context.Participants.Remove(participant);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
