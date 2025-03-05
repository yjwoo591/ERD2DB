using System;
using System.Windows.Forms;
using ERD2DB._23AsyncTask;
using ERD2DB.Windows;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase15LB;
using FDCommon.LibBase.LibBase18LB;
using FDCommon.CsBase.CsBase3CB;
using ERD2DB.CsBase.CsBase3CB;

namespace ERD2DB.Main
{
    public class Main36MA
    {
        private static readonly Logging14LB _logger = Logging14LB.Instance;
        private static readonly GlobalEventManager15LB _eventManager = GlobalEventManager15LB.Instance;
        private static readonly CanvasMessageProcessor18LB _messageProcessor = CanvasMessageProcessor18LB.Instance;

        public static void StartApplication()
        {
            try
            {
                _logger.LogMethodEntry(nameof(StartApplication), nameof(Main36MA));

                // 1. 시스템 초기화
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // 2. 초기화
                InitializeSystem();

                // 3. Canvas 모니터링 태스크 시작
                var canvasTask = CanvasMonitorTask23AT.Instance;
                canvasTask.StartMonitoring();

                // 4. Canvas 크기 모니터링 태스크 시작
                var resizeMonitor = CanvasResizeMonitorTask23AT.Instance;
                resizeMonitor.StartMonitoring();

                // 5. 메인 윈도우 생성
                var mainForm = new Main32W();

                // 6. Canvas 초기화 이벤트 발생
                _eventManager.RaiseEventAsync(
                    GlobalEventTypes3CB.CanvasCreated,
                    null,
                    new CanvasEventArgs3CB.CanvasCreatedEventArgs(
                        mainForm.ClientSize.Width,
                        mainForm.ClientSize.Height,
                        true));

                // 7. 메인 윈도우 실행
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Application startup failed: {ex.Message}", ex);
                MessageBox.Show($"응용 프로그램 시작 실패: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 8. 리소스 정리
                ShutdownSystem();
            }
        }

        private static void InitializeSystem()
        {
            try
            {
                _logger.LogMethodEntry(nameof(InitializeSystem), nameof(Main36MA));

                // 1. 설정 초기화
                InitializeConfiguration();

                // 2. 로깅 시스템 초기화
                InitializeLogging();

                // 3. 이벤트 시스템 초기화
                InitializeEventSystem();

                // 4. 캔버스 상태 초기화
                InitializeCanvasState();

                // 5. 기타 필수 서비스 초기화
                InitializeServices();

                _logger.LogInformation("System initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"System initialization failed: {ex.Message}", ex);
                throw;
            }
        }

        private static void InitializeConfiguration()
        {
            try
            {
                _logger.LogMethodEntry(nameof(InitializeConfiguration), nameof(Main36MA));
                Config36MA.Instance.LoadConfiguration();
                _logger.LogInformation("Configuration initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Configuration initialization failed: {ex.Message}", ex);
                throw;
            }
        }

        private static void InitializeLogging()
        {
            try
            {
                _logger.LogMethodEntry(nameof(InitializeLogging), nameof(Main36MA));
                _logger.LogInformation("Logging system initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging initialization failed: {ex.Message}");
                throw;
            }
        }

        private static void InitializeEventSystem()
        {
            try
            {
                _logger.LogMethodEntry(nameof(InitializeEventSystem), nameof(Main36MA));

                // Canvas 이벤트 등록
                _eventManager.RegisterEvent<CanvasEventArgs3CB.CanvasCreatedEventArgs>(
                    GlobalEventTypes3CB.CanvasCreated,
                    HandleCanvasCreated);

                // 레이아웃 이벤트 등록
                _eventManager.RegisterEvent<GlobalEventArgs3CB.LayoutEventArgs>(
                    GlobalEventTypes3CB.LayoutChanged,
                    HandleLayoutChanged);

                // 캔버스 콘텐츠 이벤트 등록
                _eventManager.RegisterEvent<GlobalEventArgs3CB.CanvasContentEventArgs>(
                    GlobalEventTypes3CB.CanvasContentUpdated,
                    HandleCanvasContentUpdated);

                _logger.LogInformation("Event system initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Event system initialization failed: {ex.Message}", ex);
                throw;
            }
        }

        private static void InitializeCanvasState()
        {
            try
            {
                _logger.LogMethodEntry(nameof(InitializeCanvasState), nameof(Main36MA));

                var canvasState = new CanvasState3CB.CanvasContainer();
                _logger.LogInformation("Canvas state initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Canvas state initialization failed: {ex.Message}", ex);
                throw;
            }
        }

        private static void InitializeServices()
        {
            try
            {
                _logger.LogMethodEntry(nameof(InitializeServices), nameof(Main36MA));

                // 캔버스 서비스 초기화
                CanvasService18LB.Instance.Dispose();  // 혹시 남아있는 인스턴스 정리

                _logger.LogInformation("Services initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Services initialization failed: {ex.Message}", ex);
                throw;
            }
        }

        private static void HandleCanvasCreated(object sender, CanvasEventArgs3CB.CanvasCreatedEventArgs e)
        {
            try
            {
                if (e.IsMainCanvas)
                {
                    _logger.LogInformation($"Main canvas created: {e.CanvasWidth}x{e.CanvasHeight}");

                    // 캔버스 메시지 프로세서에 캔버스 생성 메시지 전송
                    _messageProcessor.EnqueueMessage(new CanvasMessageProcessor18LB.CanvasMessage
                    {
                        Type = CanvasMessageProcessor18LB.CanvasMessageType.LayoutChange,
                        CanvasId = "Canvas0",
                        Data = new { Width = e.CanvasWidth, Height = e.CanvasHeight },
                        Timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling canvas created event: {ex.Message}", ex);
            }
        }

        private static void HandleLayoutChanged(object sender, GlobalEventArgs3CB.LayoutEventArgs e)
        {
            try
            {
                _logger.LogInformation($"Layout changed: {e.LayoutType}");

                // 캔버스 메시지 프로세서에 레이아웃 변경 메시지 전송
                _messageProcessor.EnqueueMessage(new CanvasMessageProcessor18LB.CanvasMessage
                {
                    Type = CanvasMessageProcessor18LB.CanvasMessageType.LayoutChange,
                    CanvasId = "Layout",
                    Data = e.LayoutData,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling layout changed event: {ex.Message}", ex);
            }
        }

        private static void HandleCanvasContentUpdated(object sender, GlobalEventArgs3CB.CanvasContentEventArgs e)
        {
            try
            {
                _logger.LogInformation($"Canvas content updated: {e.CanvasId}");

                // 캔버스 메시지 프로세서에 콘텐츠 업데이트 메시지 전송
                _messageProcessor.EnqueueMessage(new CanvasMessageProcessor18LB.CanvasMessage
                {
                    Type = CanvasMessageProcessor18LB.CanvasMessageType.FileOpen, // ContentUpdate -> FileOpen으로 수정
                    CanvasId = e.CanvasId,
                    Data = e.Content,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling canvas content update event: {ex.Message}", ex);
            }
        }

        private static void ShutdownSystem()
        {
            try
            {
                _logger.LogMethodEntry(nameof(ShutdownSystem), nameof(Main36MA));

                // 1. Canvas 모니터링 태스크 종료
                CanvasMonitorTask23AT.Instance.Shutdown();

                // 2. Canvas 크기 모니터링 태스크 종료
                CanvasResizeMonitorTask23AT.Instance.Shutdown();

                // 3. 이벤트 매니저 종료
                _eventManager.Shutdown();

                _logger.LogInformation("System shutdown completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during system shutdown: {ex.Message}", ex);
            }
        }
    }
}