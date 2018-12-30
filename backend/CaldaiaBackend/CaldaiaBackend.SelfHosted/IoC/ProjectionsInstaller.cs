using CaldaiaBackend.Application.Projections;
using CaldaiaBackend.Application.Projections.DataModels;
using CaldaiaBackend.Application.Services;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Infrastructure.Logging;
using Services.TimeSlotLoaderSaver.GDrive;
using Component = Castle.MicroKernel.Registration.Component;

namespace CaldaiaBackend.SelfHosted.IoC
{
    class ProjectionsInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component
                    .For<Last24Hours>()
                    .DependsOn(
                        Dependency.OnValue<ITimeSlotBufferLoaderSaver<AccumulatorStatistics>>(
#if DEBUG
                            new InMemoryTimeBufferLoaderSaver<AccumulatorStatistics>()
#else
                            // new FileSystemTimeSlotLoaderSaver<AccumulatorStatistics>(
                            //    ConfigurationManager.AppSettings["PathToLast24HoursJson"]
                            //    )
                            new GDriveTimeSlotLoaderSaver<AccumulatorStatistics>(
                                "CaldaiaBackend.Last24Hours.json",
                                container.Resolve<ILoggerFactory>()
                            )
#endif
                        )
                    )
                    .LifestyleSingleton(),

                Component
                    .For<Last24HoursTemperatures>()
                    .DependsOn(
                        Dependency.OnValue<ITimeSlotBufferLoaderSaver<TemperatureStatistics>>(
#if DEBUG
                            new InMemoryTimeBufferLoaderSaver<TemperatureStatistics>()
#else
                            // new FileSystemTimeSlotLoaderSaver<AccumulatorStatistics>(
                            //    ConfigurationManager.AppSettings["PathToLast24HoursTemperaturesJson"]
                            //    )
                            new GDriveTimeSlotLoaderSaver<TemperatureStatistics>(
                                "CaldaiaBackend.Last24HoursTemperatures.json",
                                container.Resolve<ILoggerFactory>()
                            )
#endif
                        )
                    )
                    .LifestyleSingleton()

                );
        }
    }
}
