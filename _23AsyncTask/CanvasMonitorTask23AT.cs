using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase15LB;
using FDCommon.LibBase.LibBase17LB;
using FDCommon.LibBase.LibBase18LB;
using FDCommon.CsBase.CsBase3CB;

namespace ERD2DB._23AsyncTask
{
    public class CanvasMonitorTask23AT : CanvasMonitorBase17LB
    {
        private static CanvasMonitorTask23AT _instance;
        private readonly CanvasMessageProcessor18LB _messageProcessor;

        public static CanvasMonitorTask23AT Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CanvasMonitorTask23AT();
                }
                return _instance;
            }
        }

        private CanvasMonitorTask23AT() : base()
        {
            _messageProcessor = CanvasMessageProcessor18LB.Instance;
        }

        protected override async Task CheckCanvasStatus()
        {
            // 기본 캔버스 상태 체크 먼저 수행
            await base.CheckCanvasStatus();

            // 추가적인 캔버스 상태 체크 및 복구 작업
            for (int i = 0; i < _canvasPanels.Length; i++)
            {
                if (_canvasPanels[i] != null && !_canvasPanels[i].Visible)
                {
                    var message = new CanvasMessageProcessor18LB.CanvasMessage
                    {
                        Type = CanvasMessageProcessor18LB.CanvasMessageType.StateChange,
                        CanvasId = $"Canvas{i + 1}",
                        Data = new { IsVisible = true },
                        Timestamp = DateTime.Now
                    };

                    _messageProcessor.EnqueueMessage(message);

                    await _eventManager.RaiseEventAsync(
                        GlobalEventTypes3CB.CanvasStateChanged,
                        this,
                        new GlobalEventArgs3CB.CanvasEventArgs
                        {
                            CanvasId = $"Canvas{i + 1}",
                            Timestamp = DateTime.Now
                        });
                }
            }
        }

        public override void SetCanvasPanel(int index, Panel panel)
        {
            base.SetCanvasPanel(index, panel);

            // 추가 작업이 필요한 경우 여기에 구현
            _logger.LogInformation($"Canvas panel {index} configured with additional settings");
        }
    }
}