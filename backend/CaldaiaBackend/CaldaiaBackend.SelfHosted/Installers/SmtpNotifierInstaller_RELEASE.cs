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
        private readonly Config _config;

        public SmtpNotifierInstaller_RELEASE(Config config)
        {
            _config = config;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var configAsJson = File.ReadAllText(_config.smtpNotifierConfigPath);
            var smtpConfig = JsonConvert.DeserializeObject<SmtpErrorNotifierConfig>(configAsJson);
            var starter = container.UseInfrastructureSmtpErrorNotifier(smtpConfig);
            starter.Start();
        }
    }
}
