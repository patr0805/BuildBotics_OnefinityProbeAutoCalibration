using Autofac;
using OnefinityProbeAutoCalibration.CalibrationProcedures;
namespace OnefinityProbeAutoCalibration.Configuration.Autofac
{
    public class ProceduresModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AutoLocateAndMeasureStockProcedure>().As<ICalibrationProcedure>();
            builder.RegisterType<CalibrateProbeOffsetsProcedure>().As<ICalibrationProcedure>();
            builder.RegisterType<DoBottomCalibrationProcedure>().As<ICalibrationProcedure>();
            builder.RegisterType<DoBottomCalibrationUsingHolesProcedure>().As<ICalibrationProcedure>();
            builder.RegisterType<DoTopCalibrationUsingHolesProcedure>().As<ICalibrationProcedure>();
            builder.RegisterType<DoTopCalibrationProcedure>().As<ICalibrationProcedure>();
            builder.RegisterType<DoFullHeightMappingProcedure>().As<ICalibrationProcedure>();
            builder.RegisterType<FindHoleCenterProcedure>().As<ICalibrationProcedure>();
        }
    }    
}
