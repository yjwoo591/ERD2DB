using System;

namespace FDCommon.CsBase.CsBase3CB
{
    public static class GlobalEventTypes3CB
    {
        // Canvas 관련 이벤트
        public const string CanvasCreated = "CanvasCreated";
        public const string CanvasResized = "CanvasResized";
        public const string CanvasVisibilityChanged = "CanvasVisibilityChanged";
        public const string CanvasStateChanged = "CanvasStateChanged";
        public const string CanvasContentUpdated = "CanvasContentUpdated";

        // 파일 관련 이벤트
        public const string FileOpened = "FileOpened";
        public const string FileSaved = "FileSaved";
        public const string FileError = "FileError";
        public const string FileLoaded = "FileLoaded";

        // 편집 관련 이벤트
        public const string ContentChanged = "ContentChanged";
        public const string FontChanged = "FontChanged";
        public const string ThemeChanged = "ThemeChanged";

        // ERD 관련 이벤트
        public const string ErdValidated = "ErdValidated";
        public const string ErdError = "ErdError";
        public const string DiagramUpdated = "DiagramUpdated";

        // 데이터베이스 관련 이벤트
        public const string DatabaseConnected = "DatabaseConnected";
        public const string DatabaseError = "DatabaseError";
        public const string TableCreated = "TableCreated";
        public const string TableModified = "TableModified";

        // 레이아웃 관련 이벤트
        public const string LayoutChanged = "LayoutChanged";
        public const string SplitterMoved = "SplitterMoved";
        public const string WindowStateChanged = "WindowStateChanged";

        // 작업 관련 이벤트
        public const string UndoPerformed = "UndoPerformed";
        public const string RedoPerformed = "RedoPerformed";
        public const string StateRestored = "StateRestored";
    }
}