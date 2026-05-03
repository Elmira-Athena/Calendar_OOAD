using Calendar.Shared.DTOs.Recurrence;
using Calendar.Shared.DTOs.Reminders;

namespace Calendar.Shared.DTOs.Appointments;

public class CreateAppointmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public bool IsAllDay { get; set; }

    /// <summary>
    /// Nếu true → tạo GroupMeeting (hiển thị cho mọi người), false → Appointment cá nhân
    /// </summary>
    public bool IsGroupMeeting { get; set; }

    /// <summary>
    /// Khi tạo Appointment cá nhân và phát hiện GroupMeeting trùng khớp nhưng user từ chối tham gia,
    /// set flag này để bỏ qua bước check matching.
    /// </summary>
    public bool SkipGroupMeetingCheck { get; set; }

    public List<ReminderDto> Reminders { get; set; } = new();
    public RecurrenceRuleDto? RecurrenceRule { get; set; }
}

public class UpdateAppointmentRequest : CreateAppointmentRequest
{
}

public class AppointmentResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public bool IsAllDay { get; set; }
    public bool IsGroupMeeting { get; set; }
    public string? CreatedByUserName { get; set; }
    public bool IsCurrentUserParticipant { get; set; }
    public bool IsCurrentUserCreator { get; set; }
    public List<ParticipantDto> Participants { get; set; } = new();
    public List<ReminderDto> Reminders { get; set; } = new();
    public RecurrenceRuleDto? RecurrenceRule { get; set; }
}

public class AppointmentListResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Color { get; set; }
    public bool IsAllDay { get; set; }
    public bool IsGroupMeeting { get; set; }
    public int ParticipantCount { get; set; }
    public bool IsCurrentUserParticipant { get; set; }
}

public class ParticipantDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTime JoinedDate { get; set; }
}

public class ConflictCheckResponse
{
    public bool HasConflict { get; set; }
    public List<AppointmentListResponse> ConflictingAppointments { get; set; } = new();
    public List<DateTime> SuggestedTimes { get; set; } = new();
}

public class GroupMeetingMatchResponse
{
    public bool IsMatch { get; set; }
    public Guid? GroupMeetingId { get; set; }
    public string? MeetingName { get; set; }
    public int ParticipantCount { get; set; }
    public bool AlreadyParticipant { get; set; }
}
