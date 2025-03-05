using System;
using System.IO;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;

namespace ERD2DB.Menu
{
    public class MenuEventHandler34ME
    {
        private static MenuEventHandler34ME _instance;
        private readonly Logging14LB _logger;

        public static MenuEventHandler34ME Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MenuEventHandler34ME();
                }
                return _instance;
            }
        }

        private MenuEventHandler34ME()
        {
            _logger = Logging14LB.Instance;
        }

        public bool InvokeHandler(MenuData34ME menuItem)
        {
            try
            {
                var handlerType = Type.GetType(menuItem.Handler_Class);
                if (handlerType == null)
                {
                    _logger.LogError($"Handler class not found: {menuItem.Handler_Class}");
                    return false;
                }

                var handlerInstance = Activator.CreateInstance(handlerType);
                var methodInfo = handlerType.GetMethod(menuItem.Handler_Method);

                if (methodInfo == null)
                {
                    _logger.LogError($"Handler method not found: {menuItem.Handler_Method}");
                    return false;
                }

                methodInfo.Invoke(handlerInstance, null);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Menu handler error: {ex.Message}", ex);
                return false;
            }
        }
    }
}