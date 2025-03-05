using System;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase17LB;
using FDCommon.LibBase.LibBase18LB;
using FDCommon.CsBase.CsBase3CB;
using FastColoredTextBoxNS;

namespace ERD2DB._23AsyncTask
{
    public class CanvasResizeMonitorTask23AT : CanvasResizeMonitorBase17LB
    {
        private static CanvasResizeMonitorTask23AT _instance;
        private readonly CanvasMessageProcessor18LB _messageProcessor;

        public static CanvasResizeMonitorTask23AT Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CanvasResizeMonitorTask23AT();
                }
                return _instance;
            }
        }

        private CanvasResizeMonitorTask23AT() : base()
        {
            _messageProcessor = CanvasMessageProcessor18LB.Instance;
        }

        protected override void CheckAndAdjustCanvasWidth()
        {
            try
            {
                // 기본 구현 호출
                base.CheckAndAdjustCanvasWidth();

                // CanvasMessageProcessor에 리사이즈 메시지 추가
                if (_erdEditor == null || _splitContainer1 == null) return;

                var maxLineWidth = GetMaxLineWidth();
                var requiredWidth = maxLineWidth + PADDING;

                // 레이어 18의 MessageProcessor 사용
                var resizeData = new CanvasMessageProcessor18LB.ResizeData
                {
                    Content = _erdEditor.Text,
                    RequiredWidth = requiredWidth,
                    CurrentWidth = _splitContainer1.Panel1.Width
                };

                _messageProcessor.EnqueueMessage(new CanvasMessageProcessor18LB.CanvasMessage
                {
                    Type = CanvasMessageProcessor18LB.CanvasMessageType.TextResize,
                    CanvasId = "Canvas1",
                    Data = resizeData,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to process resize message: {ex.Message}", ex);
            }
        }

        protected override void AdjustSplitContainers(int requiredWidth, int maxAllowedWidth)
        {
            // 기본 조정 수행
            base.AdjustSplitContainers(requiredWidth, maxAllowedWidth);

            // 추가 작업이 필요한 경우 여기에 구현
            _logger.LogInformation("Additional split container adjustments applied");
        }
    }
}