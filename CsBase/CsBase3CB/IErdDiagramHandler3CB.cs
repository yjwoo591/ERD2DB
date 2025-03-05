using System;
using System.Windows.Forms;

namespace FDCommon.CsBase.CsBase3CB
{
    public interface IErdDiagramHandler3CB
    {
        Control GetViewer();
        void UpdateZoom(bool zoomIn);
        void RenderDiagram(string content);
        void Clear();
        void Dispose();
    }
}