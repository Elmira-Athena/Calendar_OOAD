using Calendar.Core.Entities;
using Calendar.Core.Enums;
using Calendar.Infrastructure.Data;
using Calendar.Core.Interfaces;
using Calendar.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.BackgroundServices;

public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderBackgroundService> _logger;

    public ReminderBackgroundService(IServiceProvider serviceProvider, ILogger<ReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessReminders(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing reminders.");
            }

            // Check every minute
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessReminders(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CalendarDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var now = DateTime.UtcNow;

        // Fetch reminders that:
        // 1. Are not triggered
        // 2. Type is Email
        // 3. (Appointment StartTime - MinutesBefore) <= Now
        // Note: Global query filter handles !r.Appointment.IsDeleted
        var pendingReminders = await context.Reminders
            .Include(r => r.Appointment)
                .ThenInclude(a => a.User)
            .Where(r => !r.IsTriggered && r.Type == ReminderType.Email)
            .Where(r => r.Appointment.StartTime.AddMinutes(-r.MinutesBefore) <= now)
            .ToListAsync(ct);

        foreach (var reminder in pendingReminders)
        {
            try
            {
                var appointment = reminder.Appointment;
                var discriminator = context.Entry(appointment).Property("Discriminator").CurrentValue as string;

                var recipientEmails = new List<string>();
                
                if (discriminator == "GroupMeeting")
                {
                    // Send to all current participants
                    var participants = await context.Participants
                        .Include(p => p.User)
                        .Where(p => p.MeetingId == appointment.Id)
                        .ToListAsync(ct);
                    
                    recipientEmails.AddRange(participants
                        .Where(p => p.User != null && !string.IsNullOrEmpty(p.User.Email))
                        .Select(p => p.User.Email));
                }
                else
                {
                    // Regular appointment - send to owner
                    if (appointment.User != null && !string.IsNullOrEmpty(appointment.User.Email))
                    {
                        recipientEmails.Add(appointment.User.Email);
                    }
                }

                foreach (var email in recipientEmails)
                {
                    var subject = $"[NHẮC LỊCH] {appointment.Name}";
                    var body = $@"
                        <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <h2 style='color: #1976d2;'>Nhắc nhở cuộc hẹn</h2>
                            <p>Đây là thông báo nhắc nhở cho cuộc hẹn sắp tới:</p>
                            <div style='background-color: #f0f7ff; padding: 15px; border-left: 5px solid #1976d2; margin: 20px 0;'>
                                <p style='margin: 5px 0;'><strong>Cuộc họp/hẹn:</strong> {appointment.Name}</p>
                                <p style='margin: 5px 0;'><strong>Thời gian:</strong> {appointment.StartTime.ToLocalTime():dd/MM/yyyy HH:mm}</p>
                                <p style='margin: 5px 0;'><strong>Địa điểm:</strong> {appointment.Location ?? "N/A"}</p>
                            </div>
                            <p>Vui lòng chuẩn bị sẵn sàng.</p>
                            <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                            <p style='font-size: 0.9em; color: #777;'>Đây là email tự động từ hệ thống Calendar App.</p>
                        </div>";

                    await emailService.SendEmailAsync(email, subject, body);
                    _logger.LogInformation("Sent reminder email for appointment {Id} to {Email}", appointment.Id, email);
                }

                reminder.IsTriggered = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder for ID {Id}", reminder.Id);
            }
        }

        if (pendingReminders.Any())
        {
            await context.SaveChangesAsync(ct);
        }
    }
}
