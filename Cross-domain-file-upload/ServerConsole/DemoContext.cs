using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerConsole
{
    public sealed class DemoContext
    {
        private readonly int _port1;

        private readonly int _port2;

        public DemoContext(int port1, int port2)
        {
            _port1 = port1;
            _port2 = port2;
        }

        public int Server1Port
        {
            get
            {
                return _port1;
            }
        }

        public int Server2Port
        {
            get
            {
                return _port2;
            }
        }

        public IEnumerable<int> GetPorts()
        {
            yield return _port1;
            yield return _port2;
        }
    }
}