using System;
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
        private readonly Config _config;

        public ProjectionsInstaller_RELEASE(Config config)
        {
            _config = config;
        }
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                    .For<Last24HoursAccumulators>()
                    .DependsOn(
                        Dependency.OnValue<ITimeSlotBufferLoaderSaver<AccumulatorStatistics>>(
                            new FileSystemTimeSlotLoaderSaver<AccumulatorStatistics>(
                                _config.pathToLast24HoursJson,
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
                                _config.pathToLast24HoursTemperaturesJson,
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
                                _config.pathToLastWeekAccumulatorsJson,
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
                                _config.pathToLastWeekTemperaturesJson,
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
