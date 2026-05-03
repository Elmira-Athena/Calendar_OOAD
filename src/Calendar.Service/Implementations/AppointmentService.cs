using Calendar.Core.Entities;
using Calendar.Core.Exceptions;
using Calendar.Core.Interfaces;
using Calendar.Service.Interfaces;
using Calendar.Shared.Common;
using Calendar.Shared.DTOs.Appointments;
using Calendar.Shared.DTOs.Recurrence;
using Calendar.Shared.DTOs.Reminders;

namespace Calendar.Service.Implementations;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IGroupMeetingRepository _groupMeetingRepository;
    private readonly IRecurrenceRuleRepository _recurrenceRuleRepository;
    private readonly IReminderRepository _reminderRepository;
    private readonly IEmailService _emailService;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IGroupMeetingRepository groupMeetingRepository,
        IRecurrenceRuleRepository recurrenceRuleRepository,
        IReminderRepository reminderRepository,
        IEmailService emailService)
    {
        _appointmentRepository = appointmentRepository;
        _groupMeetingRepository = groupMeetingRepository;
        _recurrenceRuleRepository = recurrenceRuleRepository;
        _reminderRepository = reminderRepository;
        _emailService = emailService;
    }

    public async Task<ApiResult<List<ReminderResponse>>> GetDueRemindersAsync(Guid userId, CancellationToken ct = default)
    {
        var reminders = await _reminderRepository.GetDueRemindersAsync(userId, ct);
        var responses = new List<ReminderResponse>();

        foreach (var r in reminders)
        {
            responses.Add(new ReminderResponse
            {
                Id = r.Id,
                AppointmentId = r.AppointmentId,
                AppointmentName = r.Appointment.Name,
                StartTime = r.Appointment.StartTime,
                MinutesBefore = r.MinutesBefore,
                Type = r.Type
            });

            r.IsTriggered = true;
        }

        if (reminders.Any())
        {
            await _reminderRepository.SaveChangesAsync(ct);
        }

        return ApiResult<List<ReminderResponse>>.Ok(responses);
    }

    public async Task<ApiResult<AppointmentResponse>> CreateAsync(Guid userId, CreateAppointmentRequest request, CancellationToken ct = default)
    {
        if (request.StartTime.Kind != DateTimeKind.Utc) request.StartTime = request.StartTime.ToUniversalTime();
        if (request.EndTime.Kind != DateTimeKind.Utc) request.EndTime = request.EndTime.ToUniversalTime();

        // Conflict check on personal appointments
        var conflicts = await _appointmentRepository.GetConflictingAsync(userId, request.StartTime, request.EndTime, null, ct);
        if (conflicts.Any())
        {
            throw new AppointmentConflictException("Thời gian cuộc hẹn bị trùng với các sự kiện khác.");
        }

        // --- Case 1: Create a GroupMeeting ---
        if (request.IsGroupMeeting)
        {
            var meeting = new GroupMeeting
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedByUserId = userId,
                Name = request.Name,
                Location = request.Location,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Description = request.Description,
                Color = request.Color ?? "#7C3AED", // default purple for meetings
                IsAllDay = request.IsAllDay,
                CreatedDate = DateTime.UtcNow,
                Reminders = request.Reminders.Select(r => new Reminder
                {
                    Id = Guid.NewGuid(),
                    MinutesBefore = r.MinutesBefore,
                    Type = r.Type
                }).ToList()
            };

            // Creator auto-joins
            meeting.Participants.Add(new Participant
            {
                Id = Guid.NewGuid(),
                MeetingId = meeting.Id,
                UserId = userId,
                JoinedDate = DateTime.UtcNow
            });

            ApplyRecurrenceRule(meeting, request);

            _groupMeetingRepository.Add(meeting);
            await _groupMeetingRepository.SaveChangesAsync(ct);

            var dto = MapMeetingToResponse(meeting, userId);
            return ApiResult<AppointmentResponse>.Ok(dto);
        }

        // --- Case 2: Create a personal Appointment ---
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Location = request.Location,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Description = request.Description,
            Color = request.Color,
            IsAllDay = request.IsAllDay,
            CreatedDate = DateTime.UtcNow,
            Reminders = request.Reminders.Select(r => new Reminder
            {
                Id = Guid.NewGuid(),
                MinutesBefore = r.MinutesBefore,
                Type = r.Type
            }).ToList()
        };

        ApplyRecurrenceRule(appointment, request);

        _appointmentRepository.Add(appointment);
        await _appointmentRepository.SaveChangesAsync(ct);

        return ApiResult<AppointmentResponse>.Ok(MapToResponse(appointment));
    }

    public async Task<ApiResult<AppointmentResponse>> UpdateAsync(Guid userId, Guid id, UpdateAppointmentRequest request, CancellationToken ct = default)
    {
        var existing = await _appointmentRepository.GetByIdAsync(id, ct);
        if (existing == null || existing.UserId != userId)
        {
            throw new EntityNotFoundException("Không tìm thấy cuộc hẹn.");
        }

        // GroupMeeting can only be updated by creator
        if (existing is GroupMeeting gm && gm.CreatedByUserId != userId)
        {
            throw new DomainException("Chỉ người tạo cuộc họp mới có thể chỉnh sửa.");
        }

        if (request.StartTime.Kind != DateTimeKind.Utc) request.StartTime = request.StartTime.ToUniversalTime();
        if (request.EndTime.Kind != DateTimeKind.Utc) request.EndTime = request.EndTime.ToUniversalTime();

        var conflicts = await _appointmentRepository.GetConflictingAsync(userId, request.StartTime, request.EndTime, id, ct);
        if (conflicts.Any())
        {
            throw new AppointmentConflictException("Thời gian cuộc hẹn bị trùng với các sự kiện khác.");
        }

        existing.Name = request.Name;
        existing.Location = request.Location;
        existing.StartTime = request.StartTime;
        existing.EndTime = request.EndTime;
        existing.Description = request.Description;
        existing.Color = request.Color;
        existing.IsAllDay = request.IsAllDay;
        existing.ModifiedDate = DateTime.UtcNow;

        existing.Reminders.Clear();
        foreach (var r in request.Reminders)
        {
            existing.Reminders.Add(new Reminder { Id = Guid.NewGuid(), AppointmentId = id, MinutesBefore = r.MinutesBefore, Type = r.Type });
        }

        if (request.RecurrenceRule != null && request.RecurrenceRule.Type != Calendar.Core.Enums.RecurrenceType.None)
        {
            if (existing.RecurrenceRule == null)
                existing.RecurrenceRule = new RecurrenceRule { Id = Guid.NewGuid() };

            existing.RecurrenceRule.Type = request.RecurrenceRule.Type;
            existing.RecurrenceRule.Interval = request.RecurrenceRule.Interval;
            existing.RecurrenceRule.DayOfWeek = request.RecurrenceRule.DayOfWeek;
            existing.RecurrenceRule.DayOfMonth = request.RecurrenceRule.DayOfMonth;
            existing.RecurrenceRule.MonthOfYear = request.RecurrenceRule.MonthOfYear;
            existing.RecurrenceRule.RecurrenceEndDate = request.RecurrenceRule.RecurrenceEndDate?.ToUniversalTime();
            existing.RecurrenceRule.MaxOccurrences = request.RecurrenceRule.MaxOccurrences;
        }
        else if (existing.RecurrenceRule != null)
        {
            _recurrenceRuleRepository.Remove(existing.RecurrenceRule);
        }

        _appointmentRepository.Update(existing);
        await _appointmentRepository.SaveChangesAsync(ct);

        return ApiResult<AppointmentResponse>.Ok(MapToResponse(existing));
    }

    public async Task<ApiResult<bool>> DeleteAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        // Try GroupMeeting first to handle participant notifications
        var meeting = await _groupMeetingRepository.GetByIdWithParticipantsAsync(id, ct);
        if (meeting != null)
        {
            if (meeting.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("Chỉ người tạo mới có thể hủy cuộc họp nhóm.");

            // Send cancellation emails to participants (except the creator)
            var emailTasks = new List<Task>();
            foreach (var participant in meeting.Participants.Where(p => p.UserId != userId))
            {
                if (participant.User != null && !string.IsNullOrEmpty(participant.User.Email))
                {
                    var subject = $"[THÔNG BÁO HỦY] Cuộc họp: {meeting.Name}";
                    var body = $@"
                        <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <h2 style='color: #d9534f;'>Thông báo hủy cuộc họp</h2>
                            <p>Xin chào <strong>{participant.User.DisplayName ?? participant.User.Username}</strong>,</p>
                            <p>Chúng tôi xin thông báo rằng cuộc họp nhóm sau đây đã bị hủy bởi người tổ chức:</p>
                            <div style='background-color: #f9f9f9; padding: 15px; border-left: 5px solid #d9534f; margin: 20px 0;'>
                                <p style='margin: 5px 0;'><strong>Cuộc họp:</strong> {meeting.Name}</p>
                                <p style='margin: 5px 0;'><strong>Thời gian dự kiến:</strong> {meeting.StartTime.ToLocalTime():dd/MM/yyyy HH:mm}</p>
                            </div>
                            <p>Cuộc họp này đã được gỡ bỏ khỏi lịch của bạn. Chúng tôi xin lỗi vì sự bất tiện này.</p>
                            <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                            <p style='font-size: 0.9em; color: #777;'>Đây là email tự động từ hệ thống Calendar App. Vui lòng không phản hồi email này.</p>
                        </div>";
                    
                    emailTasks.Add(_emailService.SendEmailAsync(participant.User.Email, subject, body));
                }
            }

            if (emailTasks.Any()) await Task.WhenAll(emailTasks);

            meeting.IsDeleted = true;
            
            // Cleanup: Mark all reminders as triggered so they don't fire anymore
            foreach (var r in meeting.Reminders) r.IsTriggered = true;

            _appointmentRepository.Update(meeting);
            await _appointmentRepository.SaveChangesAsync(ct);
            return ApiResult<bool>.Ok(true);
        }

        // Regular appointment
        var existing = await _appointmentRepository.GetByIdAsync(id, ct);
        if (existing == null || existing.UserId != userId)
        {
            throw new EntityNotFoundException("Không tìm thấy cuộc hẹn.");
        }

        existing.IsDeleted = true;

        // Cleanup reminders
        foreach (var r in existing.Reminders) r.IsTriggered = true;

        _appointmentRepository.Update(existing);
        await _appointmentRepository.SaveChangesAsync(ct);

        return ApiResult<bool>.Ok(true);
    }

    public async Task<ApiResult<AppointmentResponse>> GetByIdAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        // Try GroupMeeting first (visible to everyone)
        var meeting = await _groupMeetingRepository.GetByIdWithParticipantsAsync(id, ct);
        if (meeting != null)
        {
            return ApiResult<AppointmentResponse>.Ok(MapMeetingToResponse(meeting, userId));
        }

        // Personal appointment
        var existing = await _appointmentRepository.GetByIdAsync(id, ct);
        if (existing == null || existing.UserId != userId)
        {
            throw new EntityNotFoundException("Không tìm thấy cuộc hẹn.");
        }

        return ApiResult<AppointmentResponse>.Ok(MapToResponse(existing));
    }

    public async Task<ApiResult<List<AppointmentListResponse>>> GetByDateRangeAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        // Personal appointments
        var personalAppointments = await _appointmentRepository.GetByDateRangeAsync(userId, from, to, ct);
        var result = personalAppointments.Select(x => new AppointmentListResponse
        {
            Id = x.Id,
            Name = x.Name,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            Color = x.Color,
            IsAllDay = x.IsAllDay,
            IsGroupMeeting = false
        }).ToList();

        // All GroupMeetings in range (visible to everyone)
        var groupMeetings = await _groupMeetingRepository.GetByDateRangeAsync(from, to, ct);
        var meetingItems = groupMeetings.Select(m => new AppointmentListResponse
        {
            Id = m.Id,
            Name = m.Name,
            StartTime = m.StartTime,
            EndTime = m.EndTime,
            Color = m.Color ?? "#7C3AED",
            IsAllDay = m.IsAllDay,
            IsGroupMeeting = true,
            ParticipantCount = m.Participants.Count,
            IsCurrentUserParticipant = m.Participants.Any(p => p.UserId == userId)
        });

        result.AddRange(meetingItems);
        result.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

        return ApiResult<List<AppointmentListResponse>>.Ok(result);
    }

    public async Task<ApiResult<ConflictCheckResponse>> CheckConflictAsync(Guid userId, DateTime start, DateTime end, Guid? excludeId = null, CancellationToken ct = default)
    {
        if (start.Kind != DateTimeKind.Utc) start = start.ToUniversalTime();
        if (end.Kind != DateTimeKind.Utc) end = end.ToUniversalTime();

        var conflicts = await _appointmentRepository.GetConflictingAsync(userId, start, end, excludeId, ct);

        return ApiResult<ConflictCheckResponse>.Ok(new ConflictCheckResponse
        {
            HasConflict = conflicts.Any(),
            ConflictingAppointments = conflicts.Select(x => new AppointmentListResponse
            {
                Id = x.Id,
                Name = x.Name,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Color = x.Color,
                IsAllDay = x.IsAllDay
            }).ToList(),
            SuggestedTimes = conflicts.Any() ? new List<DateTime> { end.AddMinutes(15), end.AddMinutes(30) } : new()
        });
    }

    public async Task<ApiResult<GroupMeetingMatchResponse>> CheckGroupMeetingMatchAsync(Guid userId, string name, DateTime start, DateTime end, CancellationToken ct = default)
    {
        if (start.Kind != DateTimeKind.Utc) start = start.ToUniversalTime();
        if (end.Kind != DateTimeKind.Utc) end = end.ToUniversalTime();

        var match = await _groupMeetingRepository.FindMatchingAsync(name, start, end, ct);
        if (match == null)
        {
            return ApiResult<GroupMeetingMatchResponse>.Ok(new GroupMeetingMatchResponse { IsMatch = false });
        }

        var alreadyParticipant = await _groupMeetingRepository.HasParticipantAsync(match.Id, userId, ct);

        return ApiResult<GroupMeetingMatchResponse>.Ok(new GroupMeetingMatchResponse
        {
            IsMatch = true,
            GroupMeetingId = match.Id,
            MeetingName = match.Name,
            ParticipantCount = match.Participants.Count,
            AlreadyParticipant = alreadyParticipant
        });
    }

    public async Task<ApiResult<bool>> JoinMeetingAsync(Guid userId, Guid meetingId, CancellationToken ct = default)
    {
        var meeting = await _groupMeetingRepository.GetByIdAsync(meetingId, ct);
        if (meeting == null)
            throw new EntityNotFoundException("Không tìm thấy cuộc họp nhóm.");

        var alreadyIn = await _groupMeetingRepository.HasParticipantAsync(meetingId, userId, ct);
        if (alreadyIn)
            return ApiResult<bool>.Ok(true, "Bạn đã tham gia cuộc họp này rồi.");

        _groupMeetingRepository.AddParticipant(new Participant
        {
            Id = Guid.NewGuid(),
            MeetingId = meetingId,
            UserId = userId,
            JoinedDate = DateTime.UtcNow
        });
        await _groupMeetingRepository.SaveChangesAsync(ct);

        return ApiResult<bool>.Ok(true, "Tham gia cuộc họp thành công.");
    }

    public async Task<ApiResult<bool>> LeaveMeetingAsync(Guid userId, Guid meetingId, CancellationToken ct = default)
    {
        var meeting = await _groupMeetingRepository.GetByIdAsync(meetingId, ct);
        if (meeting == null)
            throw new EntityNotFoundException("Không tìm thấy cuộc họp nhóm.");

        if (meeting.CreatedByUserId == userId)
            throw new DomainException("Người tạo cuộc họp không thể rời. Hãy xóa cuộc họp nếu muốn hủy.");

        var participant = await _groupMeetingRepository.GetParticipantAsync(meetingId, userId, ct);
        if (participant == null)
            throw new EntityNotFoundException("Bạn không phải thành viên của cuộc họp này.");

        _groupMeetingRepository.RemoveParticipant(participant);
        await _groupMeetingRepository.SaveChangesAsync(ct);

        return ApiResult<bool>.Ok(true, "Rời cuộc họp thành công.");
    }

    // ------------------- Private Helpers -------------------

    private void ApplyRecurrenceRule(Appointment appointment, CreateAppointmentRequest request)
    {
        if (request.RecurrenceRule != null && request.RecurrenceRule.Type != Calendar.Core.Enums.RecurrenceType.None)
        {
            appointment.RecurrenceRule = new RecurrenceRule
            {
                Id = Guid.NewGuid(),
                Type = request.RecurrenceRule.Type,
                Interval = request.RecurrenceRule.Interval,
                DayOfWeek = request.RecurrenceRule.DayOfWeek,
                DayOfMonth = request.RecurrenceRule.DayOfMonth,
                MonthOfYear = request.RecurrenceRule.MonthOfYear,
                RecurrenceEndDate = request.RecurrenceRule.RecurrenceEndDate?.ToUniversalTime(),
                MaxOccurrences = request.RecurrenceRule.MaxOccurrences
            };
        }
    }

    private AppointmentResponse MapToResponse(Appointment x)
    {
        return new AppointmentResponse
        {
            Id = x.Id,
            Name = x.Name,
            Location = x.Location,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            Description = x.Description,
            Color = x.Color,
            IsAllDay = x.IsAllDay,
            IsGroupMeeting = false,
            Reminders = x.Reminders.Select(r => new ReminderDto { Id = r.Id, MinutesBefore = r.MinutesBefore, Type = r.Type }).ToList(),
            RecurrenceRule = x.RecurrenceRule == null ? null : new RecurrenceRuleDto
            {
                Id = x.RecurrenceRule.Id,
                Type = x.RecurrenceRule.Type,
                Interval = x.RecurrenceRule.Interval,
                DayOfWeek = x.RecurrenceRule.DayOfWeek,
                DayOfMonth = x.RecurrenceRule.DayOfMonth,
                MonthOfYear = x.RecurrenceRule.MonthOfYear,
                RecurrenceEndDate = x.RecurrenceRule.RecurrenceEndDate,
                MaxOccurrences = x.RecurrenceRule.MaxOccurrences
            }
        };
    }

    private AppointmentResponse MapMeetingToResponse(GroupMeeting m, Guid currentUserId)
    {
        return new AppointmentResponse
        {
            Id = m.Id,
            Name = m.Name,
            Location = m.Location,
            StartTime = m.StartTime,
            EndTime = m.EndTime,
            Description = m.Description,
            Color = m.Color ?? "#7C3AED",
            IsAllDay = m.IsAllDay,
            IsGroupMeeting = true,
            CreatedByUserName = m.CreatedByUser?.DisplayName ?? m.CreatedByUser?.Username,
            IsCurrentUserParticipant = m.Participants.Any(p => p.UserId == currentUserId),
            IsCurrentUserCreator = m.CreatedByUserId == currentUserId,
            Participants = m.Participants.Select(p => new ParticipantDto
            {
                UserId = p.UserId,
                DisplayName = p.User?.DisplayName ?? p.User?.Username ?? "Unknown",
                JoinedDate = p.JoinedDate
            }).ToList(),
            Reminders = m.Reminders.Select(r => new ReminderDto { Id = r.Id, MinutesBefore = r.MinutesBefore, Type = r.Type }).ToList()
        };
    }
}
