using Calendar.Core.Enums;

namespace Calendar.Core.Entities;

public class Reminder
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public int MinutesBefore { get; set; }
    public ReminderType Type { get; set; }
    public bool IsTriggered { get; set; }

    public Appointment Appointment { get; set; } = null!;
}
