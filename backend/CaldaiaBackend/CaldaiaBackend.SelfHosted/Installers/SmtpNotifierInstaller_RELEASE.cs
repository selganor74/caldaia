using System;
using System.IO;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Infrastructure.Logging.SmtpNotification;
using Newtonsoft.Json;

namespace CaldaiaBackend.SelfHosted.Installers
{
    class SmtpNotifierInstaller_RELEASE : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var config = container.Resolve<Config>();
            container.UseInfrastructureSmtpErrorNotifier(config.smtpNotifierConfigPath);
        }
    }
}
