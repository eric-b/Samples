using System;

namespace ServiceHostAsync.Infrastructure
{
    internal sealed class NLogLogger : ILogger
    {
        private readonly NLog.Logger _logger;
        
        public NLogLogger(NLog.Logger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            _logger = logger;
        }
                
        public void Debug(string message, params object[] arguments)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            _logger.Debug(message, arguments);
        }
        
        public void Error(Exception e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
            _logger.Error(e);
        }
        
        public void Error(Exception e, string message, params object[] arguments)
        {
            if (e == null)
                throw new ArgumentNullException("e");
            if (message == null)
                throw new ArgumentNullException("message");
            _logger.Error(
                string.Format("{0}{1}{2}", arguments != null ? string.Format(message, arguments) : message, Environment.NewLine, e.ToString()));
        }
        
    }
}