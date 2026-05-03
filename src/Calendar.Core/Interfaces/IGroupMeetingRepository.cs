using Calendar.Core.Entities;

namespace Calendar.Core.Interfaces;

public interface IGroupMeetingRepository
{
    Task<GroupMeeting?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<GroupMeeting?> GetByIdWithParticipantsAsync(Guid id, CancellationToken ct = default);
    Task<List<GroupMeeting>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<GroupMeeting?> FindMatchingAsync(string name, DateTime start, DateTime end, CancellationToken ct = default);
    Task<bool> HasParticipantAsync(Guid meetingId, Guid userId, CancellationToken ct = default);
    void Add(GroupMeeting meeting);
    void AddParticipant(Participant participant);
    void RemoveParticipant(Participant participant);
    Task<Participant?> GetParticipantAsync(Guid meetingId, Guid userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
