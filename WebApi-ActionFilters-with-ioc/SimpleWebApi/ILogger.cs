using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebApi
{
    public interface ILogger
    {
        void Debug(string messageFormat, params object[] args);
    }
}
