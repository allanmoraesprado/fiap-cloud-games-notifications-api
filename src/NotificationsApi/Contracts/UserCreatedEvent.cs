namespace NotificationsApi.Contracts;

public record UserCreatedEvent(
    Guid EventId,
    Guid UserId,
    string Name,
    string Email,
    DateTime OccurredAt);
