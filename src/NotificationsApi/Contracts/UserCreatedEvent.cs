namespace NotificationsApi.Contracts;

// Canonical reference: orchestration/contracts/README.md (mirrored copy).
public record UserCreatedEvent(
    Guid EventId,
    Guid UserId,
    string Name,
    string Email,
    DateTime OccurredAt);
