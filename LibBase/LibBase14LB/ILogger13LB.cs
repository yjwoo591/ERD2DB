using System;

namespace FDCommon.LibBase.LibBase13LB
{
    public interface ILogger13LB
    {
        void LogDebug(string message);
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
        void LogMethodEntry(string methodName, string className);
        void LogMethodExit(string methodName, string className);
    }
}