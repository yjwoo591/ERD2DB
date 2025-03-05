using System;
using ERD2DB.CsBase.CsBase1CB;

namespace ERD2DB.CsBase.CsBase2CB
{
    public class CanvasBase2CB
    {
        public struct CanvasMetrics
        {
            public GeometryBase1CB.Rectangle1CB Bounds { get; set; }
            public int MinWidth { get; set; }
            public int MinHeight { get; set; }
            public int MaxWidth { get; set; }
            public int MaxHeight { get; set; }
            public float Ratio { get; set; }
        }

        public struct CanvasStyle
        {
            public byte[] BackColor { get; set; }  // RGB 값 저장 (3 bytes)
            public int Padding { get; set; }
            public bool ShowBorder { get; set; }
            public BorderStyle BorderStyle { get; set; }
            public byte[] BorderColor { get; set; }  // RGB 값 저장 (3 bytes)
        }

        public struct CanvasState
        {
            public bool IsVisible { get; set; }
            public bool IsActive { get; set; }
            public bool IsLocked { get; set; }
            public float ZoomLevel { get; set; }
            public DateTime LastModified { get; set; }
        }

        public struct CanvasTitle
        {
            public string Text { get; set; }
            public string FontName { get; set; }
            public float FontSize { get; set; }
            public bool IsBold { get; set; }
            public GeometryBase1CB.Point1CB Location { get; set; }
        }

        public enum BorderStyle
        {
            None,
            Fixed,
            Sizable
        }

        public enum CanvasType
        {
            ERDEditor,
            DatabaseEditor,
            DiagramViewer,
            PropertyEditor,
            Custom
        }

        public enum CanvasStatus
        {
            Initializing,
            Ready,
            Active,
            Error,
            Closed
        }
    }
}