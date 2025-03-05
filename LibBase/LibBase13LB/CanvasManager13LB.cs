using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using ERD2DB.CsBase.CsBase3CB;

namespace FDCommon.LibBase.LibBase13LB
{
    public class CanvasManager13LB
    {
        private static CanvasManager13LB _instance;
        private Control _mainCanvas;
        private Control[] _subCanvases;
        private readonly Dictionary<string, Control> _canvasRegistry;

        public static CanvasManager13LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CanvasManager13LB();
                }
                return _instance;
            }
        }

        private CanvasManager13LB()
        {
            _canvasRegistry = new Dictionary<string, Control>();
        }

        public Control CreateMainCanvas(Form mainForm)
        {
            try
            {
                var menuHeight = 0;
                foreach (Control control in mainForm.Controls)
                {
                    if (control is MenuStrip)
                    {
                        menuHeight = control.Height;
                        break;
                    }
                }

                _mainCanvas = new Panel
                {
                    Dock = DockStyle.Fill,
                    Location = new Point(0, menuHeight),
                    Size = new Size(
                        mainForm.ClientSize.Width,
                        mainForm.ClientSize.Height - menuHeight
                    ),
                    BackColor = Color.White,
                    Padding = new Padding(10)
                };

                mainForm.Controls.Add(_mainCanvas);
                _mainCanvas.BringToFront();
                _canvasRegistry["Canvas0"] = _mainCanvas;

                return _mainCanvas;
            }
            catch
            {
                throw;
            }
        }

        public Control[] CreateSubCanvases()
        {
            try
            {
                if (_mainCanvas == null)
                {
                    throw new InvalidOperationException("Main canvas must be created first");
                }

                _subCanvases = new Control[3];
                var width = (_mainCanvas.Width - 20) / 3;

                for (int i = 0; i < 3; i++)
                {
                    _subCanvases[i] = new Panel
                    {
                        Location = new Point(10 + (i * width), 10),
                        Size = new Size(width, _mainCanvas.Height - 20),
                        BackColor = Color.WhiteSmoke,
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    var label = new Label
                    {
                        Text = GetCanvasTitle(i),
                        Location = new Point(5, 5),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold)
                    };

                    _subCanvases[i].Controls.Add(label);
                    _mainCanvas.Controls.Add(_subCanvases[i]);
                    _canvasRegistry[$"Canvas{i + 1}"] = _subCanvases[i];
                }

                return _subCanvases;
            }
            catch
            {
                throw;
            }
        }

        private string GetCanvasTitle(int index)
        {
            return index switch
            {
                0 => "ERD Editor",
                1 => "Database Editor",
                2 => "ERD Diagram",
                _ => throw new ArgumentException("Invalid canvas index")
            };
        }

        public void AdjustCanvasLayout()
        {
            if (_mainCanvas == null || _subCanvases == null) return;

            var width = (_mainCanvas.Width - 20) / 3;
            for (int i = 0; i < _subCanvases.Length; i++)
            {
                _subCanvases[i].Location = new Point(10 + (i * width), 10);
                _subCanvases[i].Width = width;
                _subCanvases[i].Height = _mainCanvas.Height - 20;
            }
        }

        public Control GetMainCanvas()
        {
            return _mainCanvas;
        }

        public Control GetSubCanvas(int index)
        {
            if (_subCanvases == null || index < 0 || index >= _subCanvases.Length)
                return null;
            return _subCanvases[index];
        }

        public Control[] GetAllCanvases()
        {
            var allCanvases = new List<Control>();
            if (_mainCanvas != null)
                allCanvases.Add(_mainCanvas);
            if (_subCanvases != null)
                allCanvases.AddRange(_subCanvases);
            return allCanvases.ToArray();
        }

        public Control GetCanvasByName(string name)
        {
            return _canvasRegistry.TryGetValue(name, out var canvas) ? canvas : null;
        }
    }
}