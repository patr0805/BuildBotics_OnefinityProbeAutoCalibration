using OnefinityProbeAutoCalibration.CalibrationProcedures;
using OnefinityProbeAutoCalibration.Configuration;
using OnefinityProbeAutoCalibration.Services;
using System.Linq;
using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration
{
    public class Application
    {
        private IUserService _userService;
        private IOnefinityService _onefinityService;
        private ICalibrationProcedure[] _calibrationProcedures;
        private AppConfiguration _configuration;

        public Application(IUserService userService,
            IOnefinityService onefinityService,
            ICalibrationProcedure[] calibrationProcedures,
            AppConfiguration configuration)
        {
            _userService = userService;
            _onefinityService = onefinityService;
            _calibrationProcedures = calibrationProcedures;
            _configuration = configuration;
        }

        public async Task Run()
        {
            var stockWidth = _userService.RequestDistanceFromUser(_configuration.StockConfig.Width, "Enter stock width");
            var stockLength = _userService.RequestDistanceFromUser(_configuration.StockConfig.Length, "Enter stock length");
            var stockHeight = _userService.RequestDistanceFromUser(_configuration.StockConfig.Height, "Enter stock height");
            await _onefinityService.Init();

            var procedure = _calibrationProcedures.FirstOrDefault(p => p.GetName() == _configuration.Config.Type);
            if (procedure != null)
            {
                await procedure.Apply(stockWidth, stockLength, stockHeight);
            }
        }
    }
}