#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace BomberPerson.Core.Server;

/// <summary>
/// A dataflow block that receives messages associated with a realisation date.
/// It keeps the message until the realisation date and only then forwards it to the next block.
/// If a new message arrives, it replaces the one it's currently holding
/// </summary>
/// <typeparam name="T">The type of messages handled by the block.</typeparam>
public class RealisationDateBlock<T> : IPropagatorBlock<T, T>
{
    private readonly Func<T, DateTimeOffset> _dateSelector;
    private readonly BufferBlock<T> _source = new();
    private readonly object _lock = new();

    private T? _heldMessage;
    private DateTimeOffset _heldDate;
    private CancellationTokenSource? _cts;
    private bool _targetCompleted;

    public RealisationDateBlock(Func<T, DateTimeOffset> dateSelector)
    {
        _dateSelector = dateSelector ?? throw new ArgumentNullException(nameof(dateSelector));
    }

    public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T messageValue,
        ISourceBlock<T>? source, bool consumeToAccept)
    {
        if (messageValue == null) return DataflowMessageStatus.Declined;

        lock (_lock)
        {
            if (_targetCompleted) return DataflowMessageStatus.DecliningPermanently;

            DateTimeOffset messageDate = _dateSelector(messageValue);


            if (consumeToAccept)
            {
                if (source == null) return DataflowMessageStatus.NotAvailable;
                bool consumed;
                var consumedValue = source.ConsumeMessage(messageHeader, this, out consumed);
                if (!consumed) return DataflowMessageStatus.NotAvailable;
                messageValue = consumedValue!;
            }

            _heldMessage = messageValue;
            _heldDate = messageDate;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _ = ScheduleForward(messageValue, messageDate, _cts.Token);
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

            lock (_lock)
            {
                if (!token.IsCancellationRequested && EqualityComparer<T>.Default.Equals(_heldMessage, message))
                {
                    _source.Post(message);
                    _heldMessage = default;
                    _heldDate = default;

                    if (_targetCompleted)
                    {
                        _source.Complete();
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
        lock (_lock)
        {
            if (_targetCompleted) return;
            _targetCompleted = true;
            if (_heldMessage == null)
            {
                _source.Complete();
            }
        }
    }

    public void Fault(Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));
        lock (_lock)
        {
            _targetCompleted = true;
            _cts?.Cancel();
            ((ITargetBlock<T>)_source).Fault(exception);
        }
    }

    public Task Completion => _source.Completion;

    public IDisposable LinkTo(ITargetBlock<T> target, DataflowLinkOptions linkOptions)
        => _source.LinkTo(target, linkOptions);

    public T? ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target, out bool messageConsumed)
        => ((ISourceBlock<T>)_source).ConsumeMessage(messageHeader, target, out messageConsumed);

    public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        => ((ISourceBlock<T>)_source).ReleaseReservation(messageHeader, target);

    public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
        => ((ISourceBlock<T>)_source).ReserveMessage(messageHeader, target);
}