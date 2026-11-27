using MediCare.Services.Interfaces;

namespace MediCare.Web.Infrastructure;

// Every few minutes, sends the 24-hour reminder (in-app + email + SMS) for confirmed appointments.
// Interval: "Reminders:IntervalMinutes" (default 10). Turn off with "Reminders:Enabled": false.
public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IConfiguration _config;
    private readonly ILogger<ReminderBackgroundService> _logger;

    public ReminderBackgroundService(IServiceScopeFactory scopes, IConfiguration config, ILogger<ReminderBackgroundService> logger)
    {
        _scopes = scopes;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.GetValue("Reminders:Enabled", true))
            return;

        var interval = TimeSpan.FromMinutes(Math.Max(1, _config.GetValue("Reminders:IntervalMinutes", 10)));

        // Small delay so the app (and the dev database seeding) finishes starting first.
        try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); } catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var sent = await notifications.SendDueRemindersAsync(DateTime.Now);
                if (sent > 0)
                    _logger.LogInformation("Sent {Count} appointment reminder(s).", sent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The reminder run failed; it will try again next time.");
            }

            try { await Task.Delay(interval, stoppingToken); } catch (OperationCanceledException) { return; }
        }
    }
}
