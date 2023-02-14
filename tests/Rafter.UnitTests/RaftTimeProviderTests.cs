using Rafter.Impl;

namespace Rafter.UnitTests;

[TestFixture]
public class RaftTimeProviderTests
{
    [Test]
    public async Task Returns_flowing_time_success()
    {
        // Arrange
        var timeProvider = new RaftTimeProvider();

        // Act
        var firstTime = timeProvider.CurrentTime;
        await Task.Delay(TimeSpan.FromSeconds(0.1));
        var secondTime = timeProvider.CurrentTime;

        // Assert
        (secondTime - firstTime).TotalSeconds.Should().BeApproximately(0.1, 0.05);
    }
}
