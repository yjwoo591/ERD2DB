using System;
using System.Collections.Generic;
using System.Drawing;
using ERD2DB.CsBase.CsBase1CB;

namespace FDCommon.LibBase.LibBase12LB
{
    public class CanvasState12LB
    {
        public class CanvasInfo
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public GeometryBase1CB.Rectangle1CB Position { get; set; }
            public CanvasStyle Style { get; set; }
            public CanvasStatus Status { get; set; }
            public bool IsVisible { get; set; }
            public bool IsForceClosed { get; set; }

            public CanvasInfo()
            {
                Position = new GeometryBase1CB.Rectangle1CB();
                Style = new CanvasStyle();
            }
        }

        public class CanvasStyle
        {
            public Color BackColor { get; set; }
            public int Padding { get; set; }
            public bool ShowBorder { get; set; }
            public BorderStyle BorderStyle { get; set; }
            public Color BorderColor { get; set; }
        }

        public enum BorderStyle
        {
            None,
            Fixed,
            Sizable
        }

        public enum CanvasStatus
        {
            Initializing,
            Ready,
            Active,
            Error,
            Closed
        }

        public class CanvasContainer
        {
            private CanvasInfo _mainCanvas;
            private CanvasInfo[] _subCanvases;

            public CanvasInfo MainCanvas
            {
                get { return _mainCanvas; }
                set { _mainCanvas = value; }
            }

            public CanvasInfo[] SubCanvases
            {
                get { return _subCanvases; }
                set { _subCanvases = value; }
            }

            public CanvasContainer()
            {
                _mainCanvas = new CanvasInfo
                {
                    Id = "Canvas0",
                    Title = "메인 캔버스",
                    Style = new CanvasStyle
                    {
                        BackColor = Color.White,
                        Padding = 10,
                        ShowBorder = false
                    },
                    Status = CanvasStatus.Initializing
                };

                _subCanvases = new CanvasInfo[3];
                string[] titles = { "ERD 에디터", "데이터베이스 에디터", "ERD 다이어그램" };

                for (int i = 0; i < 3; i++)
                {
                    _subCanvases[i] = new CanvasInfo
                    {
                        Id = $"Canvas{i + 1}",
                        Title = titles[i],
                        Style = new CanvasStyle
                        {
                            BackColor = Color.White,
                            Padding = 5,
                            ShowBorder = true,
                            BorderStyle = BorderStyle.Sizable
                        },
                        Status = CanvasStatus.Initializing,
                        IsVisible = true
                    };
                }
            }

            public void UpdateLayout(int mainWidth, int mainHeight)
            {
                _mainCanvas.Position = new GeometryBase1CB.Rectangle1CB(
                    new GeometryBase1CB.Point1CB(0, 0),
                    new GeometryBase1CB.Size1CB(mainWidth, mainHeight)
                );

                int thirdWidth = (mainWidth - 20) / 3;
                for (int i = 0; i < _subCanvases.Length; i++)
                {
                    _subCanvases[i].Position = new GeometryBase1CB.Rectangle1CB(
                        new GeometryBase1CB.Point1CB(10 + (i * thirdWidth), 10),
                        new GeometryBase1CB.Size1CB(thirdWidth, mainHeight - 20)
                    );
                }
            }
        }
    }
}