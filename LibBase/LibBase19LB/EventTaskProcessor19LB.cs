using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase19LB
{
    public class EventTaskProcessor19LB
    {
        private static EventTaskProcessor19LB _instance;
        private readonly Logging14LB _logger;
        private readonly ConcurrentDictionary<string, Task> _backgroundTasks;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly BlockingCollection<TaskInfo> _taskQueue;

        public static EventTaskProcessor19LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EventTaskProcessor19LB();
                }
                return _instance;
            }
        }

        private class TaskInfo
        {
            public string TaskId { get; set; }
            public Func<CancellationToken, Task> Action { get; set; }
            public TaskPriority Priority { get; set; }
            public DateTime ScheduledTime { get; set; }
        }

        public enum TaskPriority
        {
            Low,
            Normal,
            High,
            Critical
        }

        private EventTaskProcessor19LB()
        {
            _logger = Logging14LB.Instance;
            _backgroundTasks = new ConcurrentDictionary<string, Task>();
            _cancellationTokenSource = new CancellationTokenSource();
            _taskQueue = new BlockingCollection<TaskInfo>();

            StartTaskProcessor();
        }

        private void StartTaskProcessor()
        {
            Task.Factory.StartNew(
                ProcessTaskQueue,
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private async Task ProcessTaskQueue()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var taskInfo = _taskQueue.Take(_cancellationTokenSource.Token);
                    await ExecuteTask(taskInfo);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing task queue: {ex.Message}", ex);
                }
            }
        }

        private async Task ExecuteTask(TaskInfo taskInfo)
        {
            try
            {
                var task = Task.Run(async () =>
                {
                    try
                    {
                        await taskInfo.Action(_cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Task execution error: {ex.Message}", ex);
                        throw;
                    }
                });

                _backgroundTasks.TryAdd(taskInfo.TaskId, task);
                await task;
                _backgroundTasks.TryRemove(taskInfo.TaskId, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing task {taskInfo.TaskId}: {ex.Message}", ex);
            }
        }

        public void RegisterBackgroundTask(string taskId, Func<CancellationToken, Task> action, TaskPriority priority = TaskPriority.Normal)
        {
            var taskInfo = new TaskInfo
            {
                TaskId = taskId,
                Action = action,
                Priority = priority,
                ScheduledTime = DateTime.Now
            };

            _taskQueue.Add(taskInfo);
            _logger.LogInformation($"Registered background task: {taskId}, Priority: {priority}");
        }

        public async Task WaitForTask(string taskId)
        {
            if (_backgroundTasks.TryGetValue(taskId, out var task))
            {
                await task;
            }
        }

        public bool IsTaskRunning(string taskId)
        {
            return _backgroundTasks.ContainsKey(taskId);
        }

        public void CancelTask(string taskId)
        {
            if (_backgroundTasks.TryGetValue(taskId, out var task))
            {
                _cancellationTokenSource.Cancel();
                _logger.LogInformation($"Cancelled task: {taskId}");
            }
        }

        public void Shutdown()
        {
            _cancellationTokenSource.Cancel();
            _taskQueue.CompleteAdding();

            try
            {
                Task.WaitAll(_backgroundTasks.Values.ToArray());
            }
            catch (AggregateException ex)
            {
                _logger.LogError($"Error during shutdown: {ex.Message}", ex);
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _taskQueue.Dispose();
            }
        }
    }
}