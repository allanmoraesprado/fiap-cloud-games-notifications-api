namespace NotificationsApi.Messaging;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:29092";
    public string UserCreatedTopic { get; set; } = "fcg.users.created";
    public string ConsumerGroup { get; set; } = "notifications-service";
}
