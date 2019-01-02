using System.Configuration;
using Application.Services;
using CaldaiaBackend.Application.Projections;
using CaldaiaBackend.Application.Projections.DataModels;
using CaldaiaBackend.Application.Services;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Infrastructure.Logging;
using Component = Castle.MicroKernel.Registration.Component;

namespace CaldaiaBackend.SelfHosted.IoC
{
    class ProjectionsInstaller_RELEASE : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                    .For<Last24HoursAccumulators>()
                    .DependsOn(
                        Dependency.OnValue<ITimeSlotBufferLoaderSaver<AccumulatorStatistics>>(
                            new FileSystemTimeSlotLoaderSaver<AccumulatorStatistics>(
                                ConfigurationManager.AppSettings["PathToLast24HoursJson"],
                                container.Resolve<ILoggerFactory>()
                                )
                        )
                    )
                    .LifestyleSingleton(),

                Component
                    .For<Last24HoursTemperatures>()
                    .DependsOn(
                        Dependency.OnValue<ITimeSlotBufferLoaderSaver<TemperatureStatistics>>(
                            new FileSystemTimeSlotLoaderSaver<TemperatureStatistics>(
                                ConfigurationManager.AppSettings["PathToLast24HoursTemperaturesJson"],
                                container.Resolve<ILoggerFactory>()
                                )
                        )
                    )
                    .LifestyleSingleton(),

                Component
                    .For<LastWeekAccumulators>()
                    .DependsOn(
                        Dependency.OnValue<ITimeSlotBufferLoaderSaver<AccumulatorStatistics>>(
                            new FileSystemTimeSlotLoaderSaver<AccumulatorStatistics>(
                                ConfigurationManager.AppSettings["PathToLastWeekAccumulatorsJson"],
                                container.Resolve<ILoggerFactory>()
                            )
                        )
                    )
                    .LifestyleSingleton(),

                Component
                    .For<LastWeekTemperatures>()
                    .DependsOn(
                        Dependency.OnValue<ITimeSlotBufferLoaderSaver<TemperatureStatistics>>(
                            new FileSystemTimeSlotLoaderSaver<TemperatureStatistics>(
                                ConfigurationManager.AppSettings["PathToLastWeekTemperaturesJson"],
                                container.Resolve<ILoggerFactory>()
                            )
                        )
                    )
                    .LifestyleSingleton()
                );
        }
    }
}
