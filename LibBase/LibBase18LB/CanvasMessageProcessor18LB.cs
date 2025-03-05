using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase16LB;

namespace FDCommon.LibBase.LibBase18LB
{
    public class CanvasMessageProcessor18LB
    {
        private static CanvasMessageProcessor18LB _instance;
        private readonly Logging14LB _logger;
        private readonly BlockingCollection<CanvasMessage> _messageQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CanvasLayoutManager16LB _layoutManager;
        private Task _processingTask;

        public static CanvasMessageProcessor18LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CanvasMessageProcessor18LB();
                }
                return _instance;
            }
        }

        public class CanvasMessage
        {
            public CanvasMessageType Type { get; set; }
            public string CanvasId { get; set; }
            public object Data { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.Now;
        }

        public enum CanvasMessageType
        {
            FileOpen,
            TextResize,
            WindowResize,
            FontChange,
            LayoutChange,
            StateChange,
            Close
        }

        private CanvasMessageProcessor18LB()
        {
            _logger = Logging14LB.Instance;
            _messageQueue = new BlockingCollection<CanvasMessage>();
            _cancellationTokenSource = new CancellationTokenSource();
            _layoutManager = CanvasLayoutManager16LB.Instance;
            StartProcessing();
        }

        private void StartProcessing()
        {
            _processingTask = Task.Run(async () =>
            {
                try
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        var message = _messageQueue.Take(_cancellationTokenSource.Token);
                        await ProcessMessage(message);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Message processing cancelled");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in message processing: {ex.Message}", ex);
                }
            }, _cancellationTokenSource.Token);
        }

        private async Task ProcessMessage(CanvasMessage message)
        {
            try
            {
                _logger.LogInformation($"Processing canvas message: {message.Type} for {message.CanvasId}");

                switch (message.Type)
                {
                    case CanvasMessageType.FileOpen:
                        await HandleFileOpen(message);
                        break;

                    case CanvasMessageType.TextResize:
                        await HandleTextResize(message);
                        break;

                    case CanvasMessageType.WindowResize:
                        await HandleWindowResize(message);
                        break;

                    case CanvasMessageType.FontChange:
                        await HandleFontChange(message);
                        break;

                    case CanvasMessageType.LayoutChange:
                        await HandleLayoutChange(message);
                        break;

                    case CanvasMessageType.StateChange:
                        await HandleStateChange(message);
                        break;

                    case CanvasMessageType.Close:
                        await HandleClose(message);
                        break;
                }

                _logger.LogInformation($"Message processed: {message.Type}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message {message.Type}: {ex.Message}", ex);
            }
        }

        private async Task HandleFileOpen(CanvasMessage message)
        {
            try
            {
                if (message.Data is string content)
                {
                    await Task.Run(() => {
                        // 문자열 내용 처리
                        _logger.LogInformation($"Processing file content with length: {content.Length}");

                        // Canvas 콘텐츠를 업데이트하는 이벤트 발생
                        var canvasIndex = int.Parse(message.CanvasId.Replace("Canvas", "")) - 1;
                        if (canvasIndex >= 0 && canvasIndex < 3)
                        {
                            _logger.LogInformation($"Updating content for canvas: {message.CanvasId}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling file open: {ex.Message}", ex);
            }
        }

        private async Task HandleTextResize(CanvasMessage message)
        {
            if (message.Data is ResizeData resizeData)
            {
                await Task.Run(() => _layoutManager.AdjustPanelWidth(
                    resizeData.Content,
                    CanvasLayoutManager16LB.ResizeReason.TextChange
                ));
            }
        }

        private async Task HandleWindowResize(CanvasMessage message)
        {
            if (message.Data is Size size)
            {
                await Task.Run(() => _layoutManager.HandleResize());
            }
        }

        private async Task HandleFontChange(CanvasMessage message)
        {
            if (message.Data is float fontSize)
            {
                await Task.Run(() => _layoutManager.AdjustPanelWidth(
                    string.Empty,
                    CanvasLayoutManager16LB.ResizeReason.FontChange
                ));
            }
        }

        private async Task HandleLayoutChange(CanvasMessage message)
        {
            await Task.Run(() => _layoutManager.HandleResize());
        }

        private async Task HandleStateChange(CanvasMessage message)
        {
            await Task.CompletedTask;
        }

        private async Task HandleClose(CanvasMessage message)
        {
            await Task.CompletedTask;
        }

        public class ResizeData
        {
            public string Content { get; set; }
            public int RequiredWidth { get; set; }
            public int CurrentWidth { get; set; }
        }

        public void EnqueueMessage(CanvasMessage message)
        {
            try
            {
                _messageQueue.Add(message);
                _logger.LogInformation($"Canvas message enqueued: {message.Type} for {message.CanvasId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to enqueue message: {ex.Message}", ex);
            }
        }

        public void Shutdown()
        {
            try
            {
                _cancellationTokenSource.Cancel();
                _messageQueue.CompleteAdding();
                _processingTask?.Wait();
                _cancellationTokenSource.Dispose();
                _messageQueue.Dispose();
                _logger.LogInformation("Canvas message processor shutdown completed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during shutdown: {ex.Message}", ex);
            }
        }
    }
}