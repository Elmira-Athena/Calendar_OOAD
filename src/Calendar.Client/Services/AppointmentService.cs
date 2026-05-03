using System.Net.Http.Json;
using Calendar.Shared.Common;
using Calendar.Shared.DTOs.Appointments;
using Calendar.Shared.DTOs.Reminders;

namespace Calendar.Client.Services;

public class AppointmentService
{
    private readonly HttpClient _httpClient;

    public AppointmentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResult<List<AppointmentListResponse>>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var fromStr = Uri.EscapeDataString(from.ToString("O"));
        var toStr = Uri.EscapeDataString(to.ToString("O"));
        var response = await _httpClient.GetAsync($"api/appointments?from={fromStr}&to={toStr}");
        var result = await response.Content.ReadFromJsonAsync<ApiResult<List<AppointmentListResponse>>>();
        return result ?? ApiResult<List<AppointmentListResponse>>.Fail("Error connecting to server.");
    }

    public async Task<ApiResult<AppointmentResponse>> GetByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/appointments/{id}");
        var result = await response.Content.ReadFromJsonAsync<ApiResult<AppointmentResponse>>();
        return result ?? ApiResult<AppointmentResponse>.Fail("Error connecting to server.");
    }

    public async Task<ApiResult<AppointmentResponse>> CreateAsync(CreateAppointmentRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/appointments", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<AppointmentResponse>>();
        return result ?? ApiResult<AppointmentResponse>.Fail("Error connecting to server.");
    }

    public async Task<ApiResult<AppointmentResponse>> UpdateAsync(Guid id, UpdateAppointmentRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/appointments/{id}", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<AppointmentResponse>>();
        return result ?? ApiResult<AppointmentResponse>.Fail("Error connecting to server.");
    }

    public async Task<ApiResult<bool>> DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/appointments/{id}");
        var result = await response.Content.ReadFromJsonAsync<ApiResult<bool>>();
        return result ?? ApiResult<bool>.Fail("Error connecting to server.");
    }

    public async Task<ApiResult<ConflictCheckResponse>> CheckConflictAsync(DateTime start, DateTime end, Guid? excludeId = null)
    {
        var startStr = Uri.EscapeDataString(start.ToString("O"));
        var endStr = Uri.EscapeDataString(end.ToString("O"));
        var excludeParam = excludeId.HasValue ? $"&excludeId={excludeId.Value}" : "";
        var response = await _httpClient.PostAsync($"api/appointments/check-conflict?start={startStr}&end={endStr}{excludeParam}", null);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<ConflictCheckResponse>>();
        return result ?? ApiResult<ConflictCheckResponse>.Fail("Error connecting to server.");
    }

    /// <summary>
    /// Kiểm tra xem tên + thời gian có trùng với GroupMeeting nào không.
    /// </summary>
    public async Task<ApiResult<GroupMeetingMatchResponse>> CheckGroupMeetingMatchAsync(string name, DateTime start, DateTime end)
    {
        var nameStr = Uri.EscapeDataString(name);
        var startStr = Uri.EscapeDataString(start.ToString("O"));
        var endStr = Uri.EscapeDataString(end.ToString("O"));
        var response = await _httpClient.PostAsync(
            $"api/appointments/check-group-match?name={nameStr}&start={startStr}&end={endStr}", null);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<GroupMeetingMatchResponse>>();
        return result ?? ApiResult<GroupMeetingMatchResponse>.Fail("Error connecting to server.");
    }

    /// <summary>
    /// Tham gia một GroupMeeting.
    /// </summary>
    public async Task<ApiResult<bool>> JoinMeetingAsync(Guid meetingId)
    {
        var response = await _httpClient.PostAsync($"api/appointments/{meetingId}/join", null);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<bool>>();
        return result ?? ApiResult<bool>.Fail("Error connecting to server.");
    }

    /// <summary>
    /// Rời khỏi một GroupMeeting.
    /// </summary>
    public async Task<ApiResult<bool>> LeaveMeetingAsync(Guid meetingId)
    {
        var response = await _httpClient.PostAsync($"api/appointments/{meetingId}/leave", null);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<bool>>();
        return result ?? ApiResult<bool>.Fail("Error connecting to server.");
    }

    public async Task<ApiResult<List<ReminderResponse>>> GetDueRemindersAsync()
    {
        var response = await _httpClient.GetAsync("api/appointments/due-reminders");
        var result = await response.Content.ReadFromJsonAsync<ApiResult<List<ReminderResponse>>>();
        return result ?? ApiResult<List<ReminderResponse>>.Fail("Error connecting to server.");
    }
}
