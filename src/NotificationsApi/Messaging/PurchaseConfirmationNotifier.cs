using NotificationsApi.Contracts;
using Microsoft.Extensions.Logging;

namespace NotificationsApi.Messaging;

// Simulates a purchase-confirmation e-mail by logging it. No SMTP / real provider.
public class PurchaseConfirmationNotifier
{
    private readonly ILogger<PurchaseConfirmationNotifier> _logger;
    public PurchaseConfirmationNotifier(ILogger<PurchaseConfirmationNotifier> logger) => _logger = logger;

    public static string Format(PaymentProcessedEvent evt) =>
        $"[PURCHASE CONFIRMATION] To user {evt.UserId} | Subject: Your FIAP Cloud Games purchase is confirmed! " +
        $"| Body: Order {evt.OrderId} for game {evt.GameId} (amount {evt.Price}) was approved.";

    public void Send(PaymentProcessedEvent evt) => _logger.LogInformation("{EmailMessage}", Format(evt));
}
