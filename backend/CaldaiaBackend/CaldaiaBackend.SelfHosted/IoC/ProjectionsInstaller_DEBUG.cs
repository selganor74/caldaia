using CaldaiaBackend.Application.Projections;
using CaldaiaBackend.Application.Projections.DataModels;
using CaldaiaBackend.Application.Services;
using CaldaiaBackend.Application.Services.Mocks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Component = Castle.MicroKernel.Registration.Component;

namespace CaldaiaBackend.SelfHosted.IoC
{
    class ProjectionsInstaller_DEBUG : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                    .For<Last24HoursAccumulators>()
                    .DependsOn(
                        Dependency.OnValue<ITimeSlotBufferLoaderSaver<AccumulatorStatistics>>(
                            new InMemoryTimeBufferLoaderSaver<AccumulatorStatistics>()
                        )
                    )
                    .LifestyleSingleton(),

                Component
                    .For<Last24HoursTemperatures>()
                    .DependsOn(
                        Dependency.OnValue<ITimeSlotBufferLoaderSaver<TemperatureStatistics>>(
                            new InMemoryTimeBufferLoaderSaver<TemperatureStatistics>()
                        )
                    )
                    .LifestyleSingleton(),

                Component
                    .For<LastWeekTemperatures>()
                    .DependsOn(
                        Dependency.OnValue<ITimeSlotBufferLoaderSaver<TemperatureStatistics>>(
                            new InMemoryTimeBufferLoaderSaver<TemperatureStatistics>()
                        )
                    )
                    .LifestyleSingleton(),

                Component
                    .For<LastWeekTemperatures>()
                    .DependsOn(
                        Dependency.OnValue<ITimeSlotBufferLoaderSaver<TemperatureStatistics>>(
                            new InMemoryTimeBufferLoaderSaver<TemperatureStatistics>()
                        )
                    )
                    .LifestyleSingleton()


                );
        }
    }
}
