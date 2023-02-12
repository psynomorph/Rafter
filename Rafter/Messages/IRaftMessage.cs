using Rafter.Values;

namespace Rafter.Messages;

public interface IRaftMessage
{
    Term Term { get; }
    PeerId PeerId { get; }
}