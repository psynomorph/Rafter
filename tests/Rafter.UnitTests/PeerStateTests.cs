using Rafter.Impl;

namespace Rafter.UnitTests;

[TestFixture]
public class PeerStateTests
{
    [Test]
    public void Create_peer_state_success()
    {
        // Arrange
        var peerInfo = new PeerInfo(
            PeerId: new PeerId(17),
            PeerUri: new Uri("https://10.0.0.17"),
            IsActive: true);

        // Act
        var state = new PeerState(peerInfo);

        // Assert
        state.Id.Should().Be(peerInfo.PeerId);
        state.Uri.Should().Be(peerInfo.PeerUri);
        state.IsActive.Should().Be(peerInfo.IsActive);
        state.LastIndex.Should().Be(LogIndex.Zero);

        state.PeerInfo.Should().BeEquivalentTo(peerInfo);
    }

    [Test]
    public void Update_peer_state_success()
    {
        // Arrange
        var state = new PeerState(new PeerInfo(
            PeerId: new PeerId(17),
            PeerUri: new Uri("https://10.0.0.17"),
            IsActive: true));

        var updatedInfo = new PeerInfo(
            PeerId: new PeerId(17),
            PeerUri: new Uri("https://10.0.0.117"),
            IsActive: false);

        // Act
        state.Update(updatedInfo);

        // Assert
        state.Id.Should().Be(updatedInfo.PeerId);
        state.Uri.Should().Be(updatedInfo.PeerUri);
        state.IsActive.Should().Be(updatedInfo.IsActive);

        state.PeerInfo.Should().BeEquivalentTo(updatedInfo);
    }

    [Test]
    public void Update_state_by_info_with_wrong_id_throwing_argument_exception()
    {
        // Arrange
        var state = new PeerState(new PeerInfo(
            PeerId: new PeerId(17),
            PeerUri: new Uri("https://10.0.0.17"),
            IsActive: true));

        var updatedInfo = new PeerInfo(
            PeerId: new PeerId(18),
            PeerUri: new Uri("https://10.0.0.117"),
            IsActive: false);

        // Act & Assert
        Action update = () => state.Update(updatedInfo);
        update.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Set_last_index_success()
    {
        // Arrange
        var state = new PeerState(new PeerInfo(
            PeerId: new PeerId(17),
            PeerUri: new Uri("https://10.0.0.17"),
            IsActive: true));

        var index = new LogIndex(9);

        // Act
        state.SetLastIndex(index);

        // Assert
        state.LastIndex.Should().Be(index);
    }

    [Test]
    public void Set_last_index_smaller_then_current_throwing_argument_exception()
    {
        // Arrange
        var state = new PeerState(new PeerInfo(
            PeerId: new PeerId(17),
            PeerUri: new Uri("https://10.0.0.17"),
            IsActive: true));
        state.SetLastIndex(18);

        var index = new LogIndex(9);

        // Act & Assert
        Action setIndex = () => state.SetLastIndex(index);
        setIndex.Should().Throw<ArgumentException>();
    }
}
