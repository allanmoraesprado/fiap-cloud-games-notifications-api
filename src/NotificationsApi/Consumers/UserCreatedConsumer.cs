using System.Text.Json;
using Confluent.Kafka;
using NotificationsApi.Contracts;
using NotificationsApi.Messaging;
using Microsoft.Extensions.Options;

namespace NotificationsApi.Consumers;

public class UserCreatedConsumer : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly WelcomeEmailNotifier _notifier;
    private readonly ILogger<UserCreatedConsumer> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public UserCreatedConsumer(IOptions<KafkaSettings> options, WelcomeEmailNotifier notifier, ILogger<UserCreatedConsumer> logger)
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
        consumer.Subscribe(_settings.UserCreatedTopic);
        _logger.LogInformation("Subscribed to {Topic} as group {Group}", _settings.UserCreatedTopic, _settings.ConsumerGroup);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> cr;
                try
                {
                    cr = consumer.Consume(stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogWarning(ex, "Consume error; retrying.");
                    continue;
                }

                if (cr?.Message?.Value is null) continue;

                try
                {
                    var evt = JsonSerializer.Deserialize<UserCreatedEvent>(cr.Message.Value, JsonOptions);
                    if (evt is not null) _notifier.Send(evt);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Malformed UserCreatedEvent; skipping message.");
                }

                // Commit after handling (skips poison messages too) -> at-least-once.
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
