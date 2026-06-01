#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace BomberPerson.Core.Server;

/// <summary>
/// A dataflow block that receives messages associated with a realization date.
/// It keeps the message until the realization date and only then forwards it to the next block.
/// If a new message arrives, it replaces the one it's currently holding
/// </summary>
/// <typeparam name="T">The type of messages handled by the block.</typeparam>
public class RealisationDateBlock<T>(Func<T, DateTimeOffset> dateSelector) : IPropagatorBlock<T, T>
{
    private readonly Func<T, DateTimeOffset> dateSelector = dateSelector ?? throw new ArgumentNullException(nameof(dateSelector));
    private readonly BufferBlock<T> source = new();
    private readonly Lock @lock = new();

    private T? heldMessage;
    private CancellationTokenSource? cts;
    private bool targetCompleted;

    public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T messageValue,
        ISourceBlock<T>? sourceParam, bool consumeToAccept)
    {
        if (messageValue == null) return DataflowMessageStatus.Declined;

        lock (@lock)
        {
            if (targetCompleted) return DataflowMessageStatus.DecliningPermanently;

            DateTimeOffset messageDate = dateSelector(messageValue);


            if (consumeToAccept)
            {
                if (sourceParam == null) return DataflowMessageStatus.NotAvailable;
                bool consumed;
                var consumedValue = sourceParam.ConsumeMessage(messageHeader, this, out consumed);
                if (!consumed) return DataflowMessageStatus.NotAvailable;
                messageValue = consumedValue!;
            }

            heldMessage = messageValue;
            cts?.Cancel();
            cts = new CancellationTokenSource();

            _ = ScheduleForward(messageValue, messageDate, cts.Token);
            return DataflowMessageStatus.Accepted;
        }
    }

    private async Task ScheduleForward(T message, DateTimeOffset date, CancellationToken token)
    {
        try
        {
            TimeSpan delay = date - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, token);
            }

            lock (@lock)
            {
                if (!token.IsCancellationRequested && EqualityComparer<T>.Default.Equals(heldMessage, message))
                {
                    source.Post(message);
                    heldMessage = default;

                    if (targetCompleted)
                    {
                        source.Complete();
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Replaced by a newer message or block was faulted/completed
        }
        catch (Exception ex)
        {
            // If an unexpected error occurs, fault the block.
            Fault(ex);
        }
    }

    public void Complete()
    {
        lock (@lock)
        {
            if (targetCompleted) return;
            targetCompleted = true;
            if (heldMessage == null)
            {
                source.Complete();
            }
        }
    }

    public void Fault(Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));
        lock (@lock)
        {
            targetCompleted = true;
            cts?.Cancel();
            ((ITargetBlock<T>)source).Fault(exception);
        }
    }

    public Task Completion => source.Completion;

    public IDisposable LinkTo(ITargetBlock<T> target, DataflowLinkOptions linkOptions)
        => source.LinkTo(target, linkOptions);

    public T? ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target, out bool messageConsumed)
        => ((ISourceBlock<T>)source).ConsumeMessage(messageHeader, target, out messageConsumed);

    public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        => ((ISourceBlock<T>)source).ReleaseReservation(messageHeader, target);

    public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        => ((ISourceBlock<T>)source).ReserveMessage(messageHeader, target);
}