using System;
using System.Collections.Generic;
using ERD2DB.CsBase.CsBase1CB;

namespace ERD2DB.CsBase.CsBase3CB
{
    public class CanvasState3CB
    {
        public class CanvasContainer
        {
            public CanvasData MainCanvas { get; set; }
            public CanvasData[] SubCanvases { get; set; }

            public CanvasContainer()
            {
                MainCanvas = new CanvasData
                {
                    CanvasId = "Canvas0",
                    CanvasName = "메인 캔버스",
                    Title = new TitleInfo { Text = "메인 캔버스" }
                };

                SubCanvases = new CanvasData[3];
                string[] names = { "ERD 에디터", "데이터베이스 에디터", "ERD 다이어그램" };

                for (int i = 0; i < 3; i++)
                {
                    SubCanvases[i] = new CanvasData
                    {
                        CanvasId = $"Canvas{i + 1}",
                        CanvasName = names[i],
                        Title = new TitleInfo { Text = names[i] }
                    };
                }
            }

            public void UpdateLayout(int mainWidth, int mainHeight)
            {
                MainCanvas.Layout = new CanvasLayout
                {
                    Bounds = new GeometryBase1CB.Rectangle1CB(
                        new GeometryBase1CB.Point1CB(0, 0),
                        new GeometryBase1CB.Size1CB(mainWidth, mainHeight)
                    )
                };

                int thirdWidth = (mainWidth - 20) / 3;
                for (int i = 0; i < SubCanvases.Length; i++)
                {
                    SubCanvases[i].Layout = new CanvasLayout
                    {
                        Bounds = new GeometryBase1CB.Rectangle1CB(
                            new GeometryBase1CB.Point1CB(10 + (i * thirdWidth), 10),
                            new GeometryBase1CB.Size1CB(thirdWidth, mainHeight - 20)
                        )
                    };
                }
            }
        }

        public class CanvasData
        {
            public string CanvasId { get; set; }
            public string CanvasName { get; set; }
            public TitleInfo Title { get; set; }
            public CanvasLayout Layout { get; set; }
            public CanvasState State { get; set; }
            public Dictionary<string, object> Properties { get; set; }

            public CanvasData()
            {
                Title = new TitleInfo();
                State = new CanvasState();
                Properties = new Dictionary<string, object>();
            }
        }

        public class TitleInfo
        {
            public string Text { get; set; }
            public string FontName { get; set; }
            public float FontSize { get; set; }
            public bool IsBold { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public TitleInfo()
            {
                FontName = "Segoe UI";
                FontSize = 9;
                IsBold = false;
            }
        }

        public class CanvasLayout
        {
            public GeometryBase1CB.Rectangle1CB Bounds { get; set; }
            public int MinWidth { get; set; }
            public int MinHeight { get; set; }
            public int MaxWidth { get; set; }
            public int MaxHeight { get; set; }
            public float Ratio { get; set; }
        }

        public class CanvasState
        {
            public bool IsVisible { get; set; }
            public bool IsActive { get; set; }
            public bool IsLocked { get; set; }
            public float ZoomLevel { get; set; }
            public DateTime LastModified { get; set; }

            public CanvasState()
            {
                IsVisible = true;
                IsActive = true;
                ZoomLevel = 1.0f;
                LastModified = DateTime.Now;
            }
        }
    }
}