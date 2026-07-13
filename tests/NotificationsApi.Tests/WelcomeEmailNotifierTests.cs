using NotificationsApi.Contracts;
using NotificationsApi.Messaging;
using FluentAssertions;
using Xunit;

namespace NotificationsApi.Tests;

public class WelcomeEmailNotifierTests
{
    [Fact]
    public void Format_includes_recipient_name_and_subject()
    {
        var evt = new UserCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "John", "john@fcg.com", DateTime.UtcNow);

        var message = WelcomeEmailNotifier.Format(evt);

        message.Should().Contain("john@fcg.com");
        message.Should().Contain("John");
        message.Should().Contain("WELCOME EMAIL");
    }
}
