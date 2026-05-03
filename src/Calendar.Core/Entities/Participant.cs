namespace Calendar.Core.Entities;

public class Participant
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedDate { get; set; }

    public GroupMeeting Meeting { get; set; } = null!;
    public User User { get; set; } = null!;
}
