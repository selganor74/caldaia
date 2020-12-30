using System;
using Castle.Windsor;
using Hangfire;

namespace CaldaiaBackend.SelfHosted.Hangfire
{
    internal class CastleJobActivator : JobActivator
    {
        private readonly IWindsorContainer _container;

        public CastleJobActivator(IWindsorContainer container)
        {
            _container = container;
        }

        public override object ActivateJob(Type jobType)
        {
            return _container.Resolve(jobType);
        }
    }
}
