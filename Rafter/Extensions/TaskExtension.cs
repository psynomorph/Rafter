using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rafter.Extensions;

internal static class TaskExtension
{
    public static async void FireAndForget(this Task task, ILogger logger)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            // Do nothing
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Task interrupted by exception");
        }
    }

    public static IEnumerable<Task<TResult>> InCompletionOrder<TResult>(this IEnumerable<Task<TResult>> tasks)
    {
        var tasksCollection = tasks as ICollection<Task<TResult>> ?? tasks.ToArray();
        var completions = new TaskCompletionSource<TResult>[tasksCollection.Count];

        for(var i = 0; i < tasksCollection.Count; i++)
        {
            completions[i] = new TaskCompletionSource<TResult>();
        }

        var taskIndex = -1;
        foreach(var task in tasks)
        {
            _ = task.ContinueWith(task =>
            {
                var taskCompletionSource = completions[Interlocked.Increment(ref taskIndex)];
                taskCompletionSource.PopulateFromCompletedTask(task);
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        return completions.Select(source => source.Task);
    }

    public static void PopulateFromCompletedTask<TResult>(this TaskCompletionSource<TResult> completionSource,  Task<TResult> task)
    {
        switch (task.Status)
        {
            case TaskStatus.RanToCompletion:
                completionSource.TrySetResult(task.Result);
                break;
            case TaskStatus.Faulted:
                completionSource.TrySetException(task.Exception.InnerExceptions);
                break;
            case TaskStatus.Canceled:
                completionSource.TrySetCanceled();
                break;
            default:
                throw new ArgumentException("Task is not in a completed state");
        }
    }
}
