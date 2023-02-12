using Rafter.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Rafter.Abstract;

public delegate void SetResponseDelegate<TResponse>(TResponse response) where TResponse : IRaftMessage;

public interface IRaftMessageListener
{
    Task OnVoteMessageReceivedAsync(
        VoteRequest message,
        SetResponseDelegate<VoteResponse> setResponse,
        CancellationToken cancellationToken);

    Task OnAppendEntriesMessageReceivedAsync(
        AppendEntriesResponse message,
        SetResponseDelegate<AppendEntriesResponse> setResponse,
        CancellationToken cancellationToken);

    Task<AppendLogResponse> OnAppendLogMessageReceivedAsync(
        AppendLogRequest message,
        SetResponseDelegate<AppendLogResponse> setResponse,
        CancellationToken cancellationToken);
}
