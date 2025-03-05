using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase15LB;
using FDCommon.CsBase.CsBase3CB;
using FDCommon.LibBase.LibBase18LB;

namespace ERD2DB._23AsyncTask
{
    public class CanvasBackgroundTask23AT
    {
        private static CanvasBackgroundTask23AT _instance;
        private readonly Logging14LB _logger;
        private readonly GlobalEventManager15LB _eventManager;
        private readonly CanvasService18LB _canvasService;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly BlockingCollection<CanvasTaskMessage> _taskQueue;
        private Task _processTask;
        private bool _isRunning;

        public static CanvasBackgroundTask23AT Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CanvasBackgroundTask23AT();
                }
                return _instance;
            }
        }

        private class CanvasTaskMessage
        {
            public string TaskType { get; set; }
            public string CanvasId { get; set; }
            public object Data { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private CanvasBackgroundTask23AT()
        {
            _logger = Logging14LB.Instance;
            _eventManager = GlobalEventManager15LB.Instance;
            _canvasService = CanvasService18LB.Instance;
            _cancellationTokenSource = new CancellationTokenSource();
            _taskQueue = new BlockingCollection<CanvasTaskMessage>();
        }

        public void StartBackgroundTask()
        {
            if (_isRunning) return;

            try
            {
                _logger.LogMethodEntry(nameof(StartBackgroundTask), nameof(CanvasBackgroundTask23AT));

                _processTask = Task.Factory.StartNew(
                    ProcessCanvasTasksAsync,
                    _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                RegisterEventHandlers();
                _isRunning = true;

                _logger.LogInformation("Canvas background task started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to start background task: {ex.Message}", ex);
                throw;
            }
        }

        private void RegisterEventHandlers()
        {
            _eventManager.RegisterEvent<GlobalEventArgs3CB.CanvasContentEventArgs>(
                GlobalEventTypes3CB.CanvasContentUpdated,
                (sender, args) => EnqueueTask("ContentUpdate", args.CanvasId, args.Content));

            _eventManager.RegisterEvent<GlobalEventArgs3CB.LayoutEventArgs>(
                GlobalEventTypes3CB.LayoutChanged,
                (sender, args) => EnqueueTask("LayoutUpdate", args.LayoutType, args.LayoutData));

            _eventManager.RegisterEvent<GlobalEventArgs3CB.FileEventArgs>(
                GlobalEventTypes3CB.FileOpened,
                (sender, args) => EnqueueTask("FileOpen", "Canvas1", args.Content));
        }

        private async Task ProcessCanvasTasksAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var message = _taskQueue.Take(_cancellationTokenSource.Token);
                    await ProcessTaskMessage(message);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing canvas task: {ex.Message}", ex);
                }
            }
        }

        private async Task ProcessTaskMessage(CanvasTaskMessage message)
        {
            try
            {
                _logger.LogInformation($"Processing canvas task: {message.TaskType} for {message.CanvasId}");

                switch (message.TaskType)
                {
                    case "ContentUpdate":
                        await UpdateCanvasContent(message.CanvasId, message.Data as string);
                        break;

                    case "LayoutUpdate":
                        await UpdateCanvasLayout(message.CanvasId, message.Data);
                        break;

                    case "FileOpen":
                        await HandleFileOpen(message.CanvasId, message.Data as string);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing task message: {ex.Message}", ex);
            }
        }

        private async Task UpdateCanvasContent(string canvasId, string content)
        {
            try
            {
                if (string.IsNullOrEmpty(content)) return;

                var canvasIndex = int.Parse(canvasId.Replace("Canvas", "")) - 1;
                var canvas = _canvasService.GetCanvas(canvasIndex);

                if (canvas != null)
                {
                    await _eventManager.RaiseEventAsync(
                        GlobalEventTypes3CB.CanvasContentUpdated,
                        this,
                        new GlobalEventArgs3CB.CanvasContentEventArgs
                        {
                            CanvasId = canvasId,
                            Content = content,
                            Timestamp = DateTime.Now
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating canvas content: {ex.Message}", ex);
            }
        }

        private async Task UpdateCanvasLayout(string canvasId, object layoutData)
        {
            try
            {
                _canvasService.HandleResize();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating canvas layout: {ex.Message}", ex);
            }
        }

        private async Task HandleFileOpen(string canvasId, string content)
        {
            try
            {
                if (string.IsNullOrEmpty(content)) return;

                var canvasIndex = int.Parse(canvasId.Replace("Canvas", "")) - 1;
                var canvas = _canvasService.GetCanvas(canvasIndex);

                if (canvas != null)
                {
                    await _eventManager.RaiseEventAsync(
                        GlobalEventTypes3CB.CanvasContentUpdated,
                        this,
                        new GlobalEventArgs3CB.CanvasContentEventArgs
                        {
                            CanvasId = canvasId,
                            Content = content,
                            Timestamp = DateTime.Now
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling file open: {ex.Message}", ex);
            }
        }

        private void EnqueueTask(string taskType, string canvasId, object data)
        {
            try
            {
                _taskQueue.Add(new CanvasTaskMessage
                {
                    TaskType = taskType,
                    CanvasId = canvasId,
                    Data = data,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to enqueue task: {ex.Message}", ex);
            }
        }

        public void Shutdown()
        {
            try
            {
                _cancellationTokenSource.Cancel();
                _taskQueue.CompleteAdding();
                _processTask?.Wait();
                _cancellationTokenSource.Dispose();
                _taskQueue.Dispose();
                _isRunning = false;
                _logger.LogInformation("Canvas background task shutdown completed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during shutdown: {ex.Message}", ex);
            }
        }
    }
}