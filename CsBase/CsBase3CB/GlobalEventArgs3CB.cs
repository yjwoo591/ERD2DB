using System;

namespace FDCommon.CsBase.CsBase3CB
{
    public class GlobalEventArgs3CB
    {
        public class CanvasEventArgs : EventArgs
        {
            public string CanvasId { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class CanvasStateEventArgs : CanvasEventArgs
        {
            public object State { get; set; }
            public string Operation { get; set; }
        }

        public class CanvasContentEventArgs : EventArgs
        {
            public string CanvasId { get; set; }
            public string Content { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class FileEventArgs : EventArgs
        {
            public string FilePath { get; set; }
            public string Content { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class FileLoadedEventArgs : EventArgs
        {
            public string FilePath { get; set; }
            public string Content { get; set; }
            public DateTime Timestamp { get; set; }
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class DatabaseEventArgs : EventArgs
        {
            public string DatabaseName { get; set; }
            public string ConnectionString { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class TableEventArgs : EventArgs
        {
            public string TableName { get; set; }
            public string DatabaseName { get; set; }
            public object TableDefinition { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class LayoutEventArgs : EventArgs
        {
            public string LayoutType { get; set; }
            public object LayoutData { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}