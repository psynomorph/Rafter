namespace Rafter.Values;

/// <summary>
/// Log meta
/// </summary>
/// <param name="LastLogTerm">Term of last log entry</param>
/// <param name="LastLogIndex">Index of last log entry</param>
public readonly record struct LogMeta(
    Term LastLogTerm,
    LogIndex LastLogIndex)
{
    /// <summary>
    /// Checks curent log entry meta is prefix of other log entry meta
    /// </summary>
    /// <param name="otherLog">Other log entry meta</param>
    public bool IsPrefixOf(LogMeta otherLog)
    {
        return otherLog.LastLogTerm >= LastLogTerm
            && otherLog.LastLogIndex >= LastLogIndex;
    }
}
