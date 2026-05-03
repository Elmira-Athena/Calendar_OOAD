using Calendar.Core.Enums;

namespace Calendar.Shared.DTOs.Reminders;

public class ReminderDto
{
    public Guid Id { get; set; }
    public int MinutesBefore { get; set; }
    public ReminderType Type { get; set; }
}

public class ReminderResponse
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public string AppointmentName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int MinutesBefore { get; set; }
    public ReminderType Type { get; set; }
}
