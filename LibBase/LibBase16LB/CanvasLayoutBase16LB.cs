using System;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase15LB;

namespace FDCommon.LibBase.LibBase16LB
{
    public class CanvasLayoutBase16LB
    {
        protected readonly Logging14LB _logger;
        protected readonly KryptonManager14LB _kryptonManager;
        protected SplitContainerEvent15LB _splitContainer1;
        protected SplitContainerEvent15LB _splitContainer2;
        protected bool _isInitializing;

        public CanvasLayoutBase16LB()
        {
            _logger = Logging14LB.Instance;
            _kryptonManager = KryptonManager14LB.Instance;
            _splitContainer1 = new SplitContainerEvent15LB();
            _splitContainer2 = new SplitContainerEvent15LB();
            _isInitializing = true;
        }

        public virtual void Initialize(Control parentControl)
        {
            try
            {
                parentControl.SuspendLayout();

                InitializeSplitContainers();
                SetupSplitContainerLayout(parentControl);

                parentControl.ResumeLayout(true);
                _isInitializing = false;

                _logger.LogInformation("Canvas layout initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize canvas layout: {ex.Message}", ex);
                throw;
            }
        }

        protected virtual void InitializeSplitContainers()
        {
            _splitContainer1.Initialize(DockStyle.Fill, Orientation.Vertical, 5);
            _splitContainer2.Initialize(DockStyle.Fill, Orientation.Vertical, 5);
        }

        protected virtual void SetupSplitContainerLayout(Control parentControl)
        {
            _splitContainer1.AddToPanel2(_splitContainer2.SplitContainer);
            parentControl.Controls.Add(_splitContainer1.SplitContainer);
        }

        public virtual void SetEqualWidths()
        {
            if (_splitContainer1.SplitContainer == null || _splitContainer2.SplitContainer == null)
                return;

            int totalWidth = _splitContainer1.SplitContainer.Width;
            int availableWidth = totalWidth - (_splitContainer1.SplitContainer.SplitterWidth * 2);
            int oneThirdWidth = availableWidth / 3;

            _splitContainer1.SetSplitterDistance(oneThirdWidth);
            _splitContainer2.SetSplitterDistance(oneThirdWidth);

            _logger.LogInformation($"Set equal widths: {oneThirdWidth}px each");
        }

        public virtual void SetSplitRatio(float ratio1, float ratio2)
        {
            if (_splitContainer1.SplitContainer == null || _splitContainer2.SplitContainer == null)
                return;

            int totalWidth = _splitContainer1.SplitContainer.Width;
            int availableWidth = totalWidth - (_splitContainer1.SplitContainer.SplitterWidth * 2);

            int firstSplitDistance = (int)(availableWidth * ratio1);
            int secondSplitDistance = (int)((totalWidth - firstSplitDistance) * ratio2);

            _splitContainer1.SetSplitterDistance(firstSplitDistance);
            _splitContainer2.SetSplitterDistance(secondSplitDistance);

            _logger.LogInformation($"Split ratios set to {ratio1:P2} and {ratio2:P2}");
        }

        public virtual void Dispose()
        {
            _splitContainer1?.Dispose();
            _splitContainer2?.Dispose();
        }
    }
}