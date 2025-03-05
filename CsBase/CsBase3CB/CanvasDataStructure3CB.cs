using System;
using System.Collections.Generic;

namespace FDCommon.CsBase.CsBase3CB
{
    public class CanvasDataStructure3CB
    {
        public struct CanvasData
        {
            public string CanvasId { get; set; }
            public string CanvasName { get; set; }
            public CanvasFunction Function { get; set; }
            public CanvasLayout Layout { get; set; }
            public CanvasState State { get; set; }
            public List<CanvasBackup> Backups { get; set; }
            public Dictionary<string, object> Properties { get; set; }
        }

        public struct CanvasFunction
        {
            public CanvasType Type { get; set; }
            public CanvasRole Role { get; set; }
            public List<string> AllowedOperations { get; set; }
            public Dictionary<string, object> FunctionConfig { get; set; }
            public CanvasPriority Priority { get; set; }
            public bool IsResizable { get; set; }
            public bool IsMovable { get; set; }
        }

        public struct CanvasLayout
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int MinWidth { get; set; }
            public int MinHeight { get; set; }
            public int MaxWidth { get; set; }
            public int MaxHeight { get; set; }
            public SizeMode SizeMode { get; set; }
            public float SizeRatio { get; set; }
            public List<LayoutConstraint> Constraints { get; set; }
            public Dictionary<string, int> Margins { get; set; }
        }

        public struct CanvasState
        {
            public bool IsVisible { get; set; }
            public bool IsActive { get; set; }
            public bool IsLocked { get; set; }
            public string CurrentMode { get; set; }
            public float ZoomLevel { get; set; }
            public DateTime LastModified { get; set; }
            public Dictionary<string, object> StateData { get; set; }
        }

        public struct CanvasBackup
        {
            public DateTime BackupTime { get; set; }
            public string BackupId { get; set; }
            public string FilePath { get; set; }
            public BackupType Type { get; set; }
            public Dictionary<string, object> BackupData { get; set; }
            public string Description { get; set; }
        }

        public struct LayoutConstraint
        {
            public string ConstraintType { get; set; }
            public string TargetCanvas { get; set; }
            public float Value { get; set; }
            public string Unit { get; set; }
            public bool IsRequired { get; set; }
        }

        public enum CanvasType
        {
            ERDEditor,
            DatabaseEditor,
            DiagramViewer,
            PropertyEditor,
            Custom
        }

        public enum CanvasRole
        {
            Editor,
            Viewer,
            Container,
            Controller
        }

        public enum CanvasPriority
        {
            High,
            Normal,
            Low
        }

        public enum SizeMode
        {
            Fixed,
            Proportional,
            Responsive,
            Dynamic
        }

        public enum BackupType
        {
            Auto,
            Manual,
            Checkpoint,
            Recovery
        }

        public struct ResizeEventData
        {
            public string SourceCanvasId { get; set; }
            public List<string> AffectedCanvasIds { get; set; }
            public Dictionary<string, CanvasLayout> NewLayouts { get; set; }
            public DateTime EventTime { get; set; }
            public string TriggerType { get; set; }
        }

        public struct CanvasRelation
        {
            public string SourceCanvasId { get; set; }
            public string TargetCanvasId { get; set; }
            public RelationType Type { get; set; }
            public Dictionary<string, object> RelationData { get; set; }
            public bool IsBidirectional { get; set; }
        }

        public enum RelationType
        {
            SizeDependent,
            ContentLinked,
            StateSync,
            Custom
        }

        public struct CanvasRestorePoint
        {
            public DateTime RestoreTime { get; set; }
            public string RestoreId { get; set; }
            public Dictionary<string, CanvasData> CanvasStates { get; set; }
            public string RestoreReason { get; set; }
            public bool IsValid { get; set; }
        }
    }
}