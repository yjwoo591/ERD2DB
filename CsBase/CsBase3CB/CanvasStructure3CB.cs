using System;
using System.Collections.Generic;

namespace FDCommon.CsBase.CsBase3CB
{
    public class CanvasStructure3CB
    {
        public struct CanvasDefinition
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public CanvasCategory Category { get; set; }
            public CanvasType Type { get; set; }
            public CanvasPriority Priority { get; set; }
            public int Order { get; set; }
            public bool IsRequired { get; set; }
            public string Description { get; set; }
        }

        public enum CanvasCategory
        {
            Main,           // 메인 캔버스
            Editor,         // 에디터 캔버스
            Viewer,         // 뷰어 캔버스
            Diagram,        // 다이어그램 캔버스
            Database,       // 데이터베이스 캔버스
            Custom          // 사용자 정의 캔버스
        }

        public enum CanvasType
        {
            Fixed,          // 고정 크기
            Proportional,   // 비율 기반
            Adaptive,       // 적응형
            Dynamic        // 동적 크기
        }

        public enum CanvasPriority
        {
            Critical = 0,   // 필수 캔버스
            High = 1,       // 높은 우선순위
            Normal = 2,     // 일반 우선순위
            Low = 3         // 낮은 우선순위
        }

        public struct CanvasLayout
        {
            public int MinWidth { get; set; }
            public int MaxWidth { get; set; }
            public int MinHeight { get; set; }
            public int MaxHeight { get; set; }
            public float PreferredRatio { get; set; }  // 너비:높이 비율
            public int DefaultSplitRatio { get; set; } // 분할 비율 (%)
            public LayoutBehavior Behavior { get; set; }
            public LayoutConstraints Constraints { get; set; }
        }

        public enum LayoutBehavior
        {
            Independent,    // 독립적 크기 조정
            Synchronized,   // 동기화된 크기 조정
            Proportional,   // 비율 유지
            Chain          // 연쇄적 크기 조정
        }

        public struct LayoutConstraints
        {
            public bool MaintainMinimumSize { get; set; }
            public bool LockAspectRatio { get; set; }
            public bool AllowOverflow { get; set; }
            public bool AutoResize { get; set; }
            public int ResizeThreshold { get; set; }
        }

        public struct CanvasState
        {
            public string CanvasId { get; set; }
            public DateTime LastModified { get; set; }
            public CanvasMetrics CurrentMetrics { get; set; }
            public CanvasMetrics PreviousMetrics { get; set; }
            public bool IsVisible { get; set; }
            public bool IsActive { get; set; }
            public float Scale { get; set; }
            public StateFlags Flags { get; set; }
        }

        public struct CanvasMetrics
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int ActualWidth { get; set; }  // 스크롤 포함
            public int ActualHeight { get; set; } // 스크롤 포함
            public int ContentWidth { get; set; }
            public int ContentHeight { get; set; }
            public int ViewportWidth { get; set; }
            public int ViewportHeight { get; set; }
        }

        [Flags]
        public enum StateFlags
        {
            None = 0,
            NeedsRedraw = 1,
            NeedsResize = 2,
            NeedsSync = 4,
            HasChanges = 8,
            IsLocked = 16,
            IsMaximized = 32,
            IsMinimized = 64
        }

        public struct CanvasBackup
        {
            public string CanvasId { get; set; }
            public DateTime BackupTime { get; set; }
            public string BackupPath { get; set; }
            public BackupType Type { get; set; }
            public Dictionary<string, object> CustomData { get; set; }
            public CanvasState State { get; set; }
            public string Version { get; set; }
            public bool IsValid { get; set; }
        }

        public enum BackupType
        {
            Auto,           // 자동 백업
            Manual,         // 수동 백업
            Checkpoint,     // 체크포인트
            Recovery       // 복구 지점
        }

        public struct CanvasRelation
        {
            public string SourceId { get; set; }
            public string TargetId { get; set; }
            public RelationType Type { get; set; }
            public RelationBehavior Behavior { get; set; }
            public bool IsBidirectional { get; set; }
            public float Priority { get; set; }
        }

        public enum RelationType
        {
            SizeDependent,     // 크기 종속
            ContentDependent,   // 내용 종속
            StateDependent,     // 상태 종속
            Independent        // 독립적
        }

        public enum RelationBehavior
        {
            Immediate,      // 즉시 반영
            Delayed,        // 지연 반영
            Queued,         // 큐 처리
            Manual         // 수동 처리
        }

        public struct CanvasGroup
        {
            public string GroupId { get; set; }
            public List<string> CanvasIds { get; set; }
            public GroupBehavior Behavior { get; set; }
            public GroupConstraints Constraints { get; set; }
            public bool IsActive { get; set; }
        }

        public enum GroupBehavior
        {
            Synchronized,   // 동기화된 동작
            Cascading,      // 연쇄적 동작
            Independent    // 독립적 동작
        }

        public struct GroupConstraints
        {
            public bool MaintainTotalSize { get; set; }
            public bool AllowReordering { get; set; }
            public bool EnforceMinimumSize { get; set; }
            public int MaximumMembers { get; set; }
        }
    }
}