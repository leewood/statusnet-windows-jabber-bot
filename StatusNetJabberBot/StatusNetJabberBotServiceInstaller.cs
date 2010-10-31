using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ServiceProcess;
using System.Configuration.Install;

namespace StatusNetJabberBot
{
    [RunInstaller(true)]
    public class StatusNetJabberBotServiceInstaller : Installer
    {

        public StatusNetJabberBotServiceInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller = 
                               new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            //# Service Account Information

            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            //# Service Information

            serviceInstaller.DisplayName = "StatusNet Jabber Bot";
            serviceInstaller.StartType = ServiceStartMode.Automatic;


            serviceInstaller.ServiceName = "StatusNetJabberBot";

            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
    }
