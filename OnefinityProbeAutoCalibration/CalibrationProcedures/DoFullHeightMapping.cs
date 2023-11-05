using OnefinityProbeAutoCalibration.Configuration;
using OnefinityProbeAutoCalibration.Models;
using OnefinityProbeAutoCalibration.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration.CalibrationProcedures
{
    public class DoFullHeightMappingProcedure : ICalibrationProcedure
    {
        private ILoggerService _loggerService;
        private ISubProcedureService _subProcedureService;
        private IMathService _mathService;
        private IOnefinityService _onefinityService;
        private AppConfiguration _configuration;
        public DoFullHeightMappingProcedure(ILoggerService loggerService,
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
            _loggerService.LogInfo("Program", "Please verify that the stock is already calibrated with procedure 'AutoLocateAndMeasureStock''");
            _loggerService.ReadLine();

            await _subProcedureService.MoveTo(0, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            // Measure left side height map
            var resultsLeft = new List<decimal>();

            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            for (var row = 2; row <= stockHeight / _configuration.Config.HeightMapResolution - 2; row += _configuration.Config.HeightMapResolution)
            {
                for (var column = 2; column <= stockWidth / _configuration.Config.HeightMapResolution - 2; column += _configuration.Config.HeightMapResolution)
                {
                    await _subProcedureService.MoveTo(-5, column, -row, _configuration.Config.ProbeFastSpeedF);
                    var probeResult = await _subProcedureService.FineProbeForEdge(true, true, true);
                    resultsLeft.Add(probeResult.X);
                }
            }
        }

        public string GetName()
        {
            return "DoFullHeightMapping";
        }
    }
}
