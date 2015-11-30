using System;

namespace ServiceHostAsync
{
    public interface ILogger
    {
         void Debug(string message, params object[] args);

        void Error(Exception ex);

        void Error(Exception ex, string message, params object[] args);
    }
}
