using System;
using System.Collections.Generic;
using System.IO;
using FDCommon.LibBase.LibBase12LB;
using FDCommon.LibBase.LibBase13LB;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase16LB
{
    public class CanvasStateManager16LB
    {
        private static CanvasStateManager16LB _instance;
        private readonly KryptonManager14LB _kryptonManager;
        private readonly Logging14LB _logger;
        private readonly CanvasManager13LB _canvasManager;
        private readonly string _stateFilePath;
        private readonly Stack<StateOperation> _undoStack;
        private readonly Stack<StateOperation> _redoStack;
        private readonly Dictionary<string, CanvasState12LB.CanvasInfo> _states;

        public static CanvasStateManager16LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CanvasStateManager16LB();
                }
                return _instance;
            }
        }

        private CanvasStateManager16LB()
        {
            _kryptonManager = KryptonManager14LB.Instance;
            _logger = Logging14LB.Instance;
            _canvasManager = CanvasManager13LB.Instance;
            _stateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CanvasStates.json");
            _undoStack = new Stack<StateOperation>();
            _redoStack = new Stack<StateOperation>();
            _states = new Dictionary<string, CanvasState12LB.CanvasInfo>();
        }

        private class StateOperation
        {
            public string CanvasId { get; set; }
            public CanvasState12LB.CanvasInfo BeforeState { get; set; }
            public CanvasState12LB.CanvasInfo AfterState { get; set; }
            public DateTime Timestamp { get; set; }
            public string OperationType { get; set; }
        }

        public void SaveState(string canvasId, CanvasState12LB.CanvasInfo state, string operationType)
        {
            try
            {
                _logger.LogMethodEntry(nameof(SaveState), nameof(CanvasStateManager16LB));

                var operation = new StateOperation
                {
                    CanvasId = canvasId,
                    BeforeState = _states.ContainsKey(canvasId) ? _states[canvasId] : null,
                    AfterState = state,
                    Timestamp = DateTime.Now,
                    OperationType = operationType
                };

                if (state != null)
                {
                    _states[canvasId] = state;
                }
                else
                {
                    _states.Remove(canvasId);
                }

                _undoStack.Push(operation);
                _redoStack.Clear();
                SaveStatesToFile();

                _logger.LogInformation($"Canvas state saved: {canvasId}, Operation: {operationType}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save state: {ex.Message}", ex);
                throw;
            }
        }

        public CanvasState12LB.CanvasInfo GetState(string canvasId)
        {
            try
            {
                _logger.LogMethodEntry(nameof(GetState), nameof(CanvasStateManager16LB));

                if (_states.TryGetValue(canvasId, out var state))
                {
                    return state;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get state: {ex.Message}", ex);
                return null;
            }
        }

        public bool Undo()
        {
            try
            {
                _logger.LogMethodEntry(nameof(Undo), nameof(CanvasStateManager16LB));

                if (_undoStack.Count == 0)
                {
                    _logger.LogInformation("No states available for undo");
                    return false;
                }

                var operation = _undoStack.Pop();
                _redoStack.Push(operation);

                if (operation.BeforeState != null)
                {
                    _states[operation.CanvasId] = operation.BeforeState;
                    RestoreState(operation.CanvasId, operation.BeforeState);
                }

                SaveStatesToFile();
                _logger.LogInformation($"Undo performed: {operation.OperationType}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to perform undo: {ex.Message}", ex);
                return false;
            }
        }

        public bool Redo()
        {
            try
            {
                _logger.LogMethodEntry(nameof(Redo), nameof(CanvasStateManager16LB));

                if (_redoStack.Count == 0)
                {
                    _logger.LogInformation("No states available for redo");
                    return false;
                }

                var operation = _redoStack.Pop();
                _undoStack.Push(operation);

                if (operation.AfterState != null)
                {
                    _states[operation.CanvasId] = operation.AfterState;
                    RestoreState(operation.CanvasId, operation.AfterState);
                }

                SaveStatesToFile();
                _logger.LogInformation($"Redo performed: {operation.OperationType}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to perform redo: {ex.Message}", ex);
                return false;
            }
        }

        private void RestoreState(string canvasId, CanvasState12LB.CanvasInfo state)
        {
            try
            {
                var canvas = _canvasManager.GetCanvasByName(canvasId);
                if (canvas != null)
                {
                    canvas.SuspendLayout();

                    var panel = _kryptonManager.CreatePanel();
                    panel.Dock = System.Windows.Forms.DockStyle.Fill;
                    panel.Location = new System.Drawing.Point(
                        state.Position.Location.X,
                        state.Position.Location.Y
                    );
                    panel.Size = new System.Drawing.Size(
                        state.Position.Size.Width,
                        state.Position.Size.Height
                    );

                    canvas.Controls.Clear();
                    canvas.Controls.Add(panel);
                    canvas.ResumeLayout(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to restore state: {ex.Message}", ex);
                throw;
            }
        }

        private void SaveStatesToFile()
        {
            try
            {
                var stateData = new
                {
                    States = _states,
                    UndoStack = _undoStack,
                    RedoStack = _redoStack
                };

                string json = System.Text.Json.JsonSerializer.Serialize(stateData);
                File.WriteAllText(_stateFilePath, json);

                _logger.LogInformation("States saved to file successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save states to file: {ex.Message}", ex);
            }
        }

        private void LoadStatesFromFile()
        {
            try
            {
                if (File.Exists(_stateFilePath))
                {
                    string json = File.ReadAllText(_stateFilePath);
                    var stateData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json);

                    // States 복원
                    _states.Clear();
                    foreach (var state in stateData.States.EnumerateObject())
                    {
                        _states[state.Name] = System.Text.Json.JsonSerializer.Deserialize<CanvasState12LB.CanvasInfo>(state.Value.ToString());
                    }

                    // Stack 재구성
                    _undoStack.Clear();
                    _redoStack.Clear();

                    foreach (var operation in stateData.UndoStack.EnumerateArray())
                    {
                        _undoStack.Push(System.Text.Json.JsonSerializer.Deserialize<StateOperation>(operation.ToString()));
                    }

                    foreach (var operation in stateData.RedoStack.EnumerateArray())
                    {
                        _redoStack.Push(System.Text.Json.JsonSerializer.Deserialize<StateOperation>(operation.ToString()));
                    }

                    _logger.LogInformation("States loaded from file successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load states from file: {ex.Message}", ex);
            }
        }
    }
}