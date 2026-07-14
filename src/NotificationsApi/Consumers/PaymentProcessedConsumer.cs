using System.Text.Json;
using Confluent.Kafka;
using NotificationsApi.Contracts;
using NotificationsApi.Messaging;
using Microsoft.Extensions.Options;

namespace NotificationsApi.Consumers;

// Consumes fcg.payments.processed (group notifications-service) and logs a purchase
// confirmation e-mail only when the payment was Approved.
public class PaymentProcessedConsumer : BackgroundService
{
    private const string Approved = "Approved";

    private readonly KafkaSettings _settings;
    private readonly PurchaseConfirmationNotifier _notifier;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PaymentProcessedConsumer(
        IOptions<KafkaSettings> options,
        PurchaseConfirmationNotifier notifier,
        ILogger<PaymentProcessedConsumer> logger)
    {
        _settings = options.Value;
        _notifier = notifier;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_settings.PaymentProcessedTopic);
        _logger.LogInformation("Subscribed to {Topic} as group {Group}", _settings.PaymentProcessedTopic, _settings.ConsumerGroup);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> cr;
                try { cr = consumer.Consume(stoppingToken); }
                catch (ConsumeException ex) { _logger.LogWarning(ex, "Consume error; retrying."); continue; }

                if (cr?.Message?.Value is null) continue;

                try
                {
                    var evt = JsonSerializer.Deserialize<PaymentProcessedEvent>(cr.Message.Value, JsonOptions);
                    if (evt is not null && string.Equals(evt.Status, Approved, StringComparison.OrdinalIgnoreCase))
                        _notifier.Send(evt);
                    // Rejected payments produce no confirmation e-mail (intentionally silent).
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Malformed PaymentProcessedEvent; skipping message.");
                }

                try { consumer.Commit(cr); }
                catch (KafkaException ex) { _logger.LogWarning(ex, "Commit failed."); }
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
        finally
        {
            consumer.Close();
        }
    }
}
