namespace Rafter.Values;

/// <summary>
/// Raft log entry
/// </summary>
/// <param name="Meta">Log entry meta</param>
/// <param name="PayloadSize">Log entry payload size</param>
/// <param name="Payload">Log entry payload</param>
public readonly record struct LogEntry(
    LogMeta Meta,
    long PayloadSize,
    byte[] Payload);