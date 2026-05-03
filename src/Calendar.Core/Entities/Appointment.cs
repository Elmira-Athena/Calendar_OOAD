namespace Calendar.Core.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public bool IsAllDay { get; set; }

    // Recurrence
    public Guid? RecurrenceRuleId { get; set; }
    public Guid? RecurrenceParentId { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public RecurrenceRule? RecurrenceRule { get; set; }
    public Appointment? RecurrenceParent { get; set; }
    public ICollection<Appointment> RecurrenceInstances { get; set; } = new List<Appointment>();
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
}
