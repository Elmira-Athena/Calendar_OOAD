using System.Security.Claims;
using Calendar.Service.Interfaces;
using Calendar.Shared.DTOs.Appointments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
    {
        var result = await _appointmentService.GetByDateRangeAsync(GetUserId(), from, to, ct);
        return Ok(result);
    }

    [HttpGet("due-reminders")]
    public async Task<IActionResult> GetDueReminders(CancellationToken ct)
    {
        var result = await _appointmentService.GetDueRemindersAsync(GetUserId(), ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.GetByIdAsync(GetUserId(), id, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        var result = await _appointmentService.CreateAsync(GetUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentRequest request, CancellationToken ct)
    {
        var result = await _appointmentService.UpdateAsync(GetUserId(), id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.DeleteAsync(GetUserId(), id, ct);
        return Ok(result);
    }

    [HttpPost("check-conflict")]
    public async Task<IActionResult> CheckConflict([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] Guid? excludeId, CancellationToken ct)
    {
        var result = await _appointmentService.CheckConflictAsync(GetUserId(), start, end, excludeId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Kiểm tra xem có GroupMeeting nào trùng tên + thời gian không.
    /// Dùng để hỏi user có muốn tham gia GroupMeeting đó không khi tạo appointment.
    /// </summary>
    [HttpPost("check-group-match")]
    public async Task<IActionResult> CheckGroupMatch([FromQuery] string name, [FromQuery] DateTime start, [FromQuery] DateTime end, CancellationToken ct)
    {
        var result = await _appointmentService.CheckGroupMeetingMatchAsync(GetUserId(), name, start, end, ct);
        return Ok(result);
    }

    /// <summary>
    /// Tham gia một GroupMeeting.
    /// </summary>
    [HttpPost("{id}/join")]
    public async Task<IActionResult> JoinMeeting(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.JoinMeetingAsync(GetUserId(), id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Rời khỏi một GroupMeeting. Creator không thể rời.
    /// </summary>
    [HttpPost("{id}/leave")]
    public async Task<IActionResult> LeaveMeeting(Guid id, CancellationToken ct)
    {
        var result = await _appointmentService.LeaveMeetingAsync(GetUserId(), id, ct);
        return Ok(result);
    }
}
