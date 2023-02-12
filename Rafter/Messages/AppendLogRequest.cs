using Rafter.Values;

namespace Rafter.Messages;

public record AppendLogRequest(
    Term Term,
    PeerId PeerId,
    int PayloadLength,
    byte[] Payload) : IRaftMessage;
