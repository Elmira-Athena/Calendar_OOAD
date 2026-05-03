using Calendar.Shared.Common;
using Calendar.Shared.DTOs.Appointments;
using Calendar.Shared.DTOs.Reminders;

namespace Calendar.Service.Interfaces;

public interface IAppointmentService
{
    Task<ApiResult<AppointmentResponse>> CreateAsync(Guid userId, CreateAppointmentRequest request, CancellationToken ct = default);
    Task<ApiResult<AppointmentResponse>> UpdateAsync(Guid userId, Guid id, UpdateAppointmentRequest request, CancellationToken ct = default);
    Task<ApiResult<bool>> DeleteAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<ApiResult<AppointmentResponse>> GetByIdAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<ApiResult<List<AppointmentListResponse>>> GetByDateRangeAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<ApiResult<ConflictCheckResponse>> CheckConflictAsync(Guid userId, DateTime start, DateTime end, Guid? excludeId = null, CancellationToken ct = default);
    Task<ApiResult<List<ReminderResponse>>> GetDueRemindersAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra xem có GroupMeeting nào trùng tên + thời gian không (để hỏi user có muốn tham gia không)
    /// </summary>
    Task<ApiResult<GroupMeetingMatchResponse>> CheckGroupMeetingMatchAsync(Guid userId, string name, DateTime start, DateTime end, CancellationToken ct = default);

    /// <summary>
    /// Người dùng tham gia GroupMeeting (thêm Participant record)
    /// </summary>
    Task<ApiResult<bool>> JoinMeetingAsync(Guid userId, Guid meetingId, CancellationToken ct = default);

    /// <summary>
    /// Người dùng rời GroupMeeting (xóa Participant record). Creator không thể rời.
    /// </summary>
    Task<ApiResult<bool>> LeaveMeetingAsync(Guid userId, Guid meetingId, CancellationToken ct = default);
}
