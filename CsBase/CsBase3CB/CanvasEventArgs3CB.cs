using System;

namespace FDCommon.CsBase.CsBase3CB
{
    public class CanvasEventArgs3CB
    {
        public class MainFormReadyEventArgs : EventArgs
        {
            public int WindowWidth { get; }
            public int WindowHeight { get; }
            public int MenuHeight { get; }

            public MainFormReadyEventArgs(int width, int height, int menuHeight)
            {
                WindowWidth = width;
                WindowHeight = height;
                MenuHeight = menuHeight;
            }
        }

        public class MenuInitializedEventArgs : EventArgs
        {
            public int MenuHeight { get; }
            public bool IsSuccess { get; }

            public MenuInitializedEventArgs(int menuHeight, bool isSuccess)
            {
                MenuHeight = menuHeight;
                IsSuccess = isSuccess;
            }
        }

        public class CanvasCreatedEventArgs : EventArgs
        {
            public int CanvasWidth { get; }
            public int CanvasHeight { get; }
            public bool IsMainCanvas { get; }

            public CanvasCreatedEventArgs(int width, int height, bool isMainCanvas)
            {
                CanvasWidth = width;
                CanvasHeight = height;
                IsMainCanvas = isMainCanvas;
            }
        }
    }
}