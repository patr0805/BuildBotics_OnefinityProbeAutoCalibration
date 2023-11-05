using Autofac;
using OnefinityProbeAutoCalibration.CalibrationProcedures;
using OnefinityProbeAutoCalibration.Services;
namespace OnefinityProbeAutoCalibration.Configuration.Autofac
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EventService>().As<IEventService>();
            builder.RegisterType<GeometryService>().As<IGeometryService>();
            builder.RegisterType<LoggerService>().As<ILoggerService>();
            builder.RegisterType<OnefinityService>().As<IOnefinityService>();
            builder.RegisterType<MathService>().As<IMathService>();
            builder.RegisterType<StateService>().As<IStateService>();
            builder.RegisterType<SubProcedureService>().As<ISubProcedureService>();
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterType<CalibrationProceduresService>().As<ICalibrationProceduresService>();
        }
    }
}
