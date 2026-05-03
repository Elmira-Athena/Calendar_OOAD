namespace Calendar.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string Email { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Participant> Participations { get; set; } = new List<Participant>();
    public ICollection<GroupMeeting> CreatedMeetings { get; set; } = new List<GroupMeeting>();
}
