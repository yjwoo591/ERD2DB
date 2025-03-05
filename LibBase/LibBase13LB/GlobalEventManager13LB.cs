using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FDCommon.CsBase.CsBase3CB;

namespace FDCommon.LibBase.LibBase13LB
{
    public class GlobalEventManager13LB
    {
        private static GlobalEventManager13LB _instance;
        private readonly ILogger3CB _logger;
        private readonly ConcurrentDictionary<string, List<Delegate>> _eventHandlers;
        private readonly BlockingCollection<EventQueueItem> _eventQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<Task> _processingTasks;
        private const int TASK_COUNT = 4;

        public static GlobalEventManager13LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalEventManager13LB();
                }
                return _instance;
            }
        }

        private class EventQueueItem
        {
            public string EventName { get; set; }
            public object Sender { get; set; }
            public object EventArgs { get; set; }
            public DateTime Timestamp { get; set; }
            public TaskCompletionSource<bool> CompletionSource { get; set; }
        }

        private GlobalEventManager13LB()
        {
            var loggerProvider = new DefaultLoggerProvider3CB();
            _logger = loggerProvider.GetLogger();
            _eventHandlers = new ConcurrentDictionary<string, List<Delegate>>();
            _eventQueue = new BlockingCollection<EventQueueItem>();
            _cancellationTokenSource = new CancellationTokenSource();
            _processingTasks = new List<Task>();

            InitializeEventProcessors();
        }

        private void InitializeEventProcessors()
        {
            for (int i = 0; i < TASK_COUNT; i++)
            {
                var task = Task.Factory.StartNew(
                    ProcessEventQueue,
                    _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                _processingTasks.Add(task);
            }

            _logger.LogInformation($"Initialized {TASK_COUNT} event processing tasks");
        }

        private async Task ProcessEventQueue()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                EventQueueItem eventItem = null;

                try
                {
                    eventItem = _eventQueue.Take(_cancellationTokenSource.Token);
                    await ProcessEvent(eventItem);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing event: {ex.Message}", ex);
                    if (eventItem?.CompletionSource != null)
                    {
                        eventItem.CompletionSource.TrySetException(ex);
                    }
                }
            }
        }

        private async Task ProcessEvent(EventQueueItem eventItem)
        {
            if (_eventHandlers.TryGetValue(eventItem.EventName, out var handlers))
            {
                foreach (var handler in handlers.ToArray())
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            handler.DynamicInvoke(eventItem.Sender, eventItem.EventArgs);
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in event handler: {ex.Message}", ex);
                    }
                }
                eventItem.CompletionSource?.TrySetResult(true);
            }
        }

        public void RegisterEvent<T>(string eventName, EventHandler<T> handler)
        {
            _eventHandlers.AddOrUpdate(
                eventName,
                new List<Delegate> { handler },
                (_, existing) =>
                {
                    existing.Add(handler);
                    return existing;
                });

            _logger.LogInformation($"Registered handler for event: {eventName}");
        }

        public void UnregisterEvent<T>(string eventName, EventHandler<T> handler)
        {
            if (_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                handlers.Remove(handler);
                _logger.LogInformation($"Unregistered handler for event: {eventName}");
            }
        }

        public async Task RaiseEventAsync<T>(string eventName, object sender, T eventArgs)
        {
            var completionSource = new TaskCompletionSource<bool>();
            var queueItem = new EventQueueItem
            {
                EventName = eventName,
                Sender = sender,
                EventArgs = eventArgs,
                Timestamp = DateTime.Now,
                CompletionSource = completionSource
            };

            _eventQueue.Add(queueItem);
            await completionSource.Task;
        }

        public void Shutdown()
        {
            _cancellationTokenSource.Cancel();
            Task.WaitAll(_processingTasks.ToArray());
            _eventQueue.CompleteAdding();
            _cancellationTokenSource.Dispose();
            _eventQueue.Dispose();
        }
    }
}