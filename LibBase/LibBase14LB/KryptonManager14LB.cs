using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Krypton.Toolkit;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase14LB
{
    public class KryptonManager14LB
    {
        private static KryptonManager14LB _instance;
        private readonly Logging14LB _logger;
        private readonly Dictionary<string, Control> _controlRegistry;
        private KryptonPanel _mainPanel;

        public static KryptonManager14LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new KryptonManager14LB();
                }
                return _instance;
            }
        }

        private KryptonManager14LB()
        {
            _logger = Logging14LB.Instance;
            _controlRegistry = new Dictionary<string, Control>();
        }

        public KryptonForm CreateKryptonForm()
        {
            try
            {
                var form = new KryptonForm();
                RegisterControl(form);
                return form;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create KryptonForm", ex);
                throw;
            }
        }

        public KryptonPanel CreatePanel()
        {
            try
            {
                var panel = new KryptonPanel();
                panel.StateNormal.Color1 = Color.White;
                RegisterControl(panel);
                return panel;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create panel", ex);
                throw;
            }
        }

        public KryptonComboBox CreateComboBox()
        {
            try
            {
                var comboBox = new KryptonComboBox();
                RegisterControl(comboBox);
                return comboBox;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create combo box", ex);
                throw;
            }
        }

        public KryptonDataGridView CreateDataGridView()
        {
            try
            {
                var grid = new KryptonDataGridView();
                RegisterControl(grid);
                return grid;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create data grid view", ex);
                throw;
            }
        }

        public KryptonButton CreateButton()
        {
            try
            {
                var button = new KryptonButton();
                button.StateCommon.Back.Color1 = Color.FromArgb(250, 252, 252);
                button.StateCommon.Border.Color1 = Color.FromArgb(220, 220, 220);
                RegisterControl(button);
                return button;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create button", ex);
                throw;
            }
        }

        public KryptonLabel CreateLabel()
        {
            try
            {
                var label = new KryptonLabel();
                label.StateCommon.ShortText.Font = new Font("Segoe UI", 9F);
                RegisterControl(label);
                return label;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create label", ex);
                throw;
            }
        }

        public KryptonTextBox CreateTextBox()
        {
            try
            {
                var textBox = new KryptonTextBox();
                textBox.StateCommon.Border.Color1 = Color.FromArgb(220, 220, 220);
                RegisterControl(textBox);
                return textBox;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create textbox", ex);
                throw;
            }
        }

        public MenuStrip CreateMenuStrip()
        {
            try
            {
                var menuStrip = new MenuStrip();
                RegisterControl(menuStrip);
                return menuStrip;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create menu strip", ex);
                throw;
            }
        }

        public ToolStrip CreateToolStrip()
        {
            try
            {
                var toolStrip = new ToolStrip();
                RegisterControl(toolStrip);
                return toolStrip;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create tool strip", ex);
                throw;
            }
        }

        public StatusStrip CreateStatusStrip()
        {
            try
            {
                var statusStrip = new StatusStrip();
                RegisterControl(statusStrip);
                return statusStrip;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create status strip", ex);
                throw;
            }
        }

        public ToolStripMenuItem CreateMenuItem(string text)
        {
            try
            {
                var menuItem = new ToolStripMenuItem(text);
                return menuItem;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create menu item", ex);
                throw;
            }
        }

        public ToolStripStatusLabel CreateStatusLabel()
        {
            try
            {
                var statusLabel = new ToolStripStatusLabel();
                return statusLabel;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create status label", ex);
                throw;
            }
        }

        private void RegisterControl(Control control)
        {
            var key = $"{control.GetType().Name}_{DateTime.Now.Ticks}";
            _controlRegistry[key] = control;
        }

        public KryptonPanel GetMainPanel()
        {
            if (_mainPanel == null)
            {
                throw new InvalidOperationException("Main panel has not been initialized");
            }
            return _mainPanel;
        }

        public void InitializeMainPanel(Form mainForm)
        {
            try
            {
                _mainPanel = CreatePanel();
                _mainPanel.Dock = DockStyle.Fill;
                mainForm.Controls.Add(_mainPanel);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize main panel", ex);
                throw;
            }
        }
    }
}