using System;
using System.Configuration;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Component = Castle.MicroKernel.Registration.Component;

using Application.Services;
using Infrastructure.Logging;

using CaldaiaBackend.Application.Projections;
using CaldaiaBackend.Application.Projections.DataModels;
using CaldaiaBackend.Application.Services;

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
                                TimeSpan.FromMinutes(10),
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
                                TimeSpan.FromMinutes(10),
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
                                TimeSpan.FromMinutes(20),
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
                                TimeSpan.FromMinutes(20),
                                container.Resolve<ILoggerFactory>()
                            )
                        )
                    )
                    .LifestyleSingleton()
                );
        }
    }
}
