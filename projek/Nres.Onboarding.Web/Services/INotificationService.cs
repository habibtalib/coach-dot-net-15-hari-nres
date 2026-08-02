namespace Nres.Onboarding.Web.Services;

/// <summary>
/// Sends a notification to a recipient. Kept as an interface so the training
/// implementation can be swapped for a real SMTP or queue-backed sender in production
/// without touching a single controller.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Notify a user. Takes an Identity USER ID, not an email address: the transport
    /// decides how to reach them. Kumpulan 1 adds an SMTP implementation on Hari 10-12
    /// by ADDING a class, not editing this one.
    /// </summary>
    Task NotifyAsync(string toUserId, string subject, string body,
        CancellationToken ct = default);
}

/// <summary>
/// Training implementation: writes the notification to the console so participants can
/// see exactly when and why a body would be sent, with no mail server to configure.
/// </summary>
public class ConsoleNotificationService : INotificationService
{
    private readonly ILogger<ConsoleNotificationService> _logger;

    public ConsoleNotificationService(ILogger<ConsoleNotificationService> logger)
    {
        _logger = logger;
    }

    public Task NotifyAsync(string toUserId, string subject, string body,
        CancellationToken ct = default)
    {
        Console.WriteLine($"To: {toUserId} | {subject} | {body}");

        // Also written to the logger so the notification survives in log files.
        _logger.LogInformation(
            "Notification queued. To: {Recipient} | Subject: {Subject}",
            toUserId,
            subject);

        return Task.CompletedTask;
    }
}
