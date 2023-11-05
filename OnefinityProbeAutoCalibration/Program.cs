using Autofac;
using Microsoft.Extensions.Configuration;
using OnefinityProbeAutoCalibration.Configuration;
using OnefinityProbeAutoCalibration.Configuration.Autofac;
using System.IO;
using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CompositionRoot().Resolve<Application>().Run();
        }

        private static IContainer CompositionRoot()
        {
            var configBuilder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appSettings.json", optional: false);
            var configBuild = configBuilder.Build();
            var config = configBuild.GetSection("Config").Get<AppConfiguration>();
            var stockConfig = configBuild.GetSection("Config").GetSection("StockConfig").Get<StockConfiguration>();
            var calibrationConfig = configBuild.GetSection("Config").GetSection("CalibrationConfig").Get<CalibrationConfiguration>();
            var generelConfig = configBuild.GetSection("Config").GetSection("GenerelConfig").Get<GenerelConfig>();
            config.CalibrationConfig = calibrationConfig;
            config.StockConfig = stockConfig;
            config.Config = generelConfig;

            var builder = new ContainerBuilder();
            builder.RegisterInstance(config);
            builder.RegisterType<Application>();
            builder.RegisterModule<ServicesModule>();
            builder.RegisterModule<ProceduresModule>();
            return builder.Build();
        }
        
    }
}
