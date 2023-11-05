using OnefinityProbeAutoCalibration.Configuration;
using OnefinityProbeAutoCalibration.Models;
using OnefinityProbeAutoCalibration.Services;
using System;
using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration.CalibrationProcedures
{
    public class FindHoleCenterProcedure : ICalibrationProcedure
    {
        private ILoggerService _loggerService;
        private ISubProcedureService _subProcedureService;
        private IMathService _mathService;
        private IOnefinityService _onefinityService;
        private AppConfiguration _configuration;
        public FindHoleCenterProcedure(ILoggerService loggerService,
            ISubProcedureService subProcedureService,
            IMathService mathService,
            IOnefinityService onefinityService,
            AppConfiguration configuration)
        {
            _loggerService = loggerService;
            _subProcedureService = subProcedureService;
            _mathService = mathService;
            _configuration = configuration;
            _onefinityService = onefinityService;
        }

        public async Task Apply(decimal stockWidth, decimal stockLength, decimal stockHeight)
        {            
            _loggerService.LogInfo("Program", "Please navigate to hole");
            _loggerService.ReadLine();
            _loggerService.LogInfo("Program", "Probing center");
            await _subProcedureService.CalibrateCenter(true); // Calibrate twice to improve acuracy
            await _subProcedureService.CalibrateCenterVH(true); // Calibrate third time to improve acuracy
            var center = await _subProcedureService.CalibrateCenterVH(true);
            _loggerService.LogInfo("Result", "Center x: " + center.X);
            _loggerService.LogInfo("Result", "Center y: " + center.Y);
        }

        public string GetName()
        {
            return "FindHoleCenter";
        }
    }
}
