using Rafter.Impl;

namespace Rafter.UnitTests;

[TestFixture]
public class PeerStorageTests
{
    [Test]
    public void Create_peers_in_storage_success()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
            new PeerInfo(4, new Uri("http://10.0.0.4"), true),
            new PeerInfo(5, new Uri("http://10.0.0.5"), false),
        };
        var storage = new PeersStorage();

        // Act
        storage.UpdatePeers(peers);

        // Assert
        storage
            .GetAllPeers()
            .Select(peerState => peerState.PeerInfo)
            .Should().BeEquivalentTo(peers);

        storage.QuorumSize.Should().Be(3);
    }

    [Test]
    public void Updates_peers_in_storage_success()
    {
        // Arrange
        var oldPeers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
            new PeerInfo(4, new Uri("http://10.0.0.4"), true),
            new PeerInfo(5, new Uri("http://10.0.0.5"), false),
        };

        var newPeers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.1.1"), false),
            new PeerInfo(2, new Uri("http://10.0.1.2"), true),
            new PeerInfo(3, new Uri("http://10.0.1.3"), false),
            new PeerInfo(4, new Uri("http://10.0.1.4"), true),
            new PeerInfo(5, new Uri("http://10.0.1.5"), true),
        };

        var storage = new PeersStorage();
        storage.UpdatePeers(oldPeers);

        // Act
        storage.UpdatePeers(newPeers);

        // Assert
        storage
            .GetAllPeers()
            .Select(peerState => peerState.PeerInfo)
            .Should().BeEquivalentTo(newPeers);

        storage.QuorumSize.Should().Be(2);
    }

    [Test]
    public void Returns_peer_success()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
            new PeerInfo(4, new Uri("http://10.0.0.4"), true),
            new PeerInfo(5, new Uri("http://10.0.0.5"), false),
        };
        var storage = new PeersStorage();
        storage.UpdatePeers(peers);

        // Act
        var peer = storage.GetPeer(3);

        // Assert
        peer.PeerInfo.Should().Be(peers[2]);
    }

    [Test]
    public void Throws_argument_exception_for_wrong_peer_id()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
            new PeerInfo(4, new Uri("http://10.0.0.4"), true),
            new PeerInfo(5, new Uri("http://10.0.0.5"), false),
        };
        var storage = new PeersStorage();
        storage.UpdatePeers(peers);

        // Act & assert
        Action getPeer = () => storage.GetPeer(7);
        getPeer.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Returns_active_peers()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
            new PeerInfo(4, new Uri("http://10.0.0.4"), true),
            new PeerInfo(5, new Uri("http://10.0.0.5"), false),
        };
        var storage = new PeersStorage();
        storage.UpdatePeers(peers);

        // Act
        var activePeers = storage.GetOtherActivePeers();

        // Assert
        activePeers.Should().OnlyContain(peer => peer.IsActive);
    }

    [Test]
    public void Filter_current_peer()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
            new PeerInfo(4, new Uri("http://10.0.0.4"), true),
            new PeerInfo(5, new Uri("http://10.0.0.5"), false),
        };
        var storage = new PeersStorage();
        storage.UpdatePeers(peers);

        var currentPeerId = peers[0].PeerId;
        storage.SetCurrentPeerId(currentPeerId);

        // Act
        var activePeers = storage.GetOtherActivePeers();

        // Assert
        activePeers.Should().OnlyContain(peer => peer.IsActive);
        activePeers.Should().NotContain(peer => peer.Id == currentPeerId);
    }
}
