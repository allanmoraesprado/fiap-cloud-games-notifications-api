using NotificationsApi.Contracts;
using NotificationsApi.Messaging;
using FluentAssertions;
using Xunit;

namespace NotificationsApi.Tests;

public class PurchaseConfirmationNotifierTests
{
    [Fact]
    public void Format_includes_confirmation_order_and_game()
    {
        var evt = new PaymentProcessedEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 49.90m, "Approved", DateTime.UtcNow);

        var message = PurchaseConfirmationNotifier.Format(evt);

        message.Should().Contain("PURCHASE CONFIRMATION");
        message.Should().Contain(evt.OrderId.ToString());
        message.Should().Contain(evt.GameId.ToString());
    }
}
