using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleWebApi.Infrastructure
{
    public class Logger : ILogger
    {
        public void Debug(string messageFormat, params object[] args)
        {
            if (args!=null)
                System.Diagnostics.Debug.WriteLine(messageFormat, args);
            else
                System.Diagnostics.Debug.WriteLine(messageFormat);
        }
    }
}