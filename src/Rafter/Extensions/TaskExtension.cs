using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Timer = System.Timers.Timer;

namespace Rafter.Extensions;

internal static class TaskExtension
{
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

    public static Task<ValueTuple<T1, T2>> Zip<T1, T2>(this Task<T1> task, T2 value)
    {
        var completionSource = new TaskCompletionSource<ValueTuple<T1, T2>>();
        _ = task.ContinueWith(task => 
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    completionSource.TrySetResult(ValueTuple.Create(task.Result, value));
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
        });
        return completionSource.Task;
    }

    public static Task AsEllapsedOnceTask(this Timer timer, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource();

        void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Elapsed -= Timer_Elapsed;
            tcs.SetResult();
        }

        cancellationToken.Register(() =>
        {
            try
            {
                timer.Elapsed -= Timer_Elapsed;
            } 
            catch (Exception) 
            { 
                // Ignore exception
            }
            tcs.TrySetCanceled();
        });
        timer.Elapsed += Timer_Elapsed;
        
        return tcs.Task;
    }
}
