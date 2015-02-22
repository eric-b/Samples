using System;
using System.Linq;
using System.Web.Security;
using Microsoft.Owin.Security.DataProtection;

namespace Identity2.Sample1.Shared.Owin
{
    public class MachineKeyDataProtector : IDataProtector
    {
        private readonly string[] _purposes;

        public MachineKeyDataProtector(params string[] purposes)
        {
            if (purposes == null)
                throw new ArgumentNullException("purposes");
            _purposes = purposes;
        }

        public byte[] Protect(byte[] userData)
        {
            return MachineKey.Protect(userData, _purposes);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return MachineKey.Unprotect(protectedData, _purposes);
        }
    }
}