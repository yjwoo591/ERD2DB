using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FDCommon.LibBase.LibBase14LB
{
    public static class ControlExtensions14LB
    {
        public static Task InvokeAsync(this Control control, Action action)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            if (control.InvokeRequired)
            {
                control.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        action();
                        taskCompletionSource.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.SetException(ex);
                    }
                }));
            }
            else
            {
                try
                {
                    action();
                    taskCompletionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            }

            return taskCompletionSource.Task;
        }
    }
}