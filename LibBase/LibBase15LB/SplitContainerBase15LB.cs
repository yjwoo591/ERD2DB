using System;
using System.Windows.Forms;
using System.Drawing;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase15LB
{
    public class SplitContainerBase15LB
    {
        protected readonly Logging14LB _logger;
        protected readonly KryptonManager14LB _kryptonManager;
        protected SplitContainer _splitContainer;

        public SplitContainer SplitContainer => _splitContainer;

        public SplitContainerBase15LB()
        {
            _logger = Logging14LB.Instance;
            _kryptonManager = KryptonManager14LB.Instance;
        }

        public virtual void Initialize(DockStyle dockStyle, Orientation orientation, int splitterWidth)
        {
            try
            {
                _splitContainer = new SplitContainer
                {
                    Dock = dockStyle,
                    Orientation = orientation,
                    SplitterWidth = splitterWidth
                };

                SetDefaultProperties();
                _logger.LogInformation("SplitContainer initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize SplitContainer: {ex.Message}", ex);
                throw;
            }
        }

        protected virtual void SetDefaultProperties()
        {
            _splitContainer.Panel1MinSize = 100;
            _splitContainer.Panel2MinSize = 100;
            _splitContainer.IsSplitterFixed = false;
        }

        public virtual void SetSplitterDistance(int distance)
        {
            if (_splitContainer != null)
            {
                try
                {
                    if (_splitContainer.InvokeRequired)
                    {
                        _splitContainer.Invoke(new Action(() => {
                            _splitContainer.SplitterDistance = distance;
                        }));
                    }
                    else
                    {
                        _splitContainer.SplitterDistance = distance;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to set splitter distance: {ex.Message}", ex);
                }
            }
        }

        public virtual int GetSplitterDistance()
        {
            return _splitContainer?.SplitterDistance ?? 0;
        }

        public virtual void SetMinimumSize(int panel1MinSize, int panel2MinSize)
        {
            if (_splitContainer != null)
            {
                _splitContainer.Panel1MinSize = panel1MinSize;
                _splitContainer.Panel2MinSize = panel2MinSize;
            }
        }

        public virtual void AddToPanel1(Control control)
        {
            _splitContainer?.Panel1.Controls.Add(control);
        }

        public virtual void AddToPanel2(Control control)
        {
            _splitContainer?.Panel2.Controls.Add(control);
        }

        public virtual void Dispose()
        {
            _splitContainer?.Dispose();
        }
    }
}