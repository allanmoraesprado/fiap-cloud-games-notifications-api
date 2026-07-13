using NotificationsApi.Contracts;
using Microsoft.Extensions.Logging;

namespace NotificationsApi.Messaging;

// Simulates sending a welcome e-mail by logging it to the console.
// No SMTP / real provider — this is intentional for the academic MVP.
public class WelcomeEmailNotifier
{
    private readonly ILogger<WelcomeEmailNotifier> _logger;
    public WelcomeEmailNotifier(ILogger<WelcomeEmailNotifier> logger) => _logger = logger;

    public static string Format(UserCreatedEvent evt) =>
        $"[WELCOME EMAIL] To: {evt.Email} | Subject: Welcome to FIAP Cloud Games, {evt.Name}! " +
        $"| Body: Your account (id {evt.UserId}) was created successfully.";

    public void Send(UserCreatedEvent evt) => _logger.LogInformation("{EmailMessage}", Format(evt));
}
