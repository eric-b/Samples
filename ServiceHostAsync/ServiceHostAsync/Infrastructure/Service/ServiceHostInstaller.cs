using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ServiceHostAsync.Infrastructure.Service
{
    [RunInstaller(true)]
    public sealed class ServiceHostInstaller : Installer
    {
        public ServiceHostInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            serviceInstaller.DisplayName = "ServiceHostAsync (sample)";
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = ServiceHost<IDisposable>.ServiceName;

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
