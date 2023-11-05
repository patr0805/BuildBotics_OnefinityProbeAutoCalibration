using OnefinityProbeAutoCalibration.Configuration;
using OnefinityProbeAutoCalibration.Models;
using OnefinityProbeAutoCalibration.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration.CalibrationProcedures
{
    public class CalibrateProbeOffsetsProcedure : ICalibrationProcedure
    {
        private ILoggerService _loggerService;
        private ISubProcedureService _subProcedureService;
        private IOnefinityService _onefinityService;
        private AppConfiguration _configuration;
        public CalibrateProbeOffsetsProcedure(ILoggerService loggerService,
            ISubProcedureService subProcedureService,
            IOnefinityService onefinityService,
            AppConfiguration configuration)
        {
            _loggerService = loggerService;
            _subProcedureService = subProcedureService;
            _configuration = configuration;
            _onefinityService = onefinityService;
        }

        public async Task Apply(decimal stockWidth, decimal stockLength, decimal stockHeight)
        {
            _loggerService.LogInfo("Program", "Have you calibrated the zero x,y to be the bottom left corner of the stock? If not, close program now!");
            _loggerService.ReadLine();

            var stockTopPos = new Point2D(10m, 10m, 0m);
            await _subProcedureService.MoveTo(10, 10, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            _loggerService.LogInfo("Program", "Roughly measuring stock");
            await _subProcedureService.ProbeTowards(null, null, _configuration.Config.ZCalibrationHitLimit, _configuration.Config.ProbeSpeedF);
            stockTopPos.Z = _onefinityService.GetCurrentRelativePosition().Z;

            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.MoveTo(10, 10, null, _configuration.Config.ProbeFastSpeedF);

            await _subProcedureService.RoughlyProbeForEdge(true, false, 0);
            var right_estimatedLeftH = await _subProcedureService.FineProbeForEdge(true, true);
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            await _subProcedureService.MoveTo(right_estimatedLeftH.X + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(false, false, 0);
            var forward_estimatedLeftVBottom = await _subProcedureService.FineProbeForEdge(false, true);

            Console.WriteLine("Please rotate the probe 180 degrees");
            Console.ReadLine();

            await _subProcedureService.MoveTo(right_estimatedLeftH.X + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, 10, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(true, false, 0);
            var left_estimatedLeftH = await _subProcedureService.FineProbeForEdge(true, true);

            await _subProcedureService.MoveTo(right_estimatedLeftH.X + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, forward_estimatedLeftVBottom.Y - _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(false, false, 0);
            var backwards_estimatedLeftVBottom = await _subProcedureService.FineProbeForEdge(false, true);

            Console.WriteLine("Please return the probe to 0 degrees");
            Console.ReadLine();

            var probeHeightOffset = stockTopPos.Z;
            var probeForwardOffset = -forward_estimatedLeftVBottom.Y;
            var probeBackwardsOffset = -backwards_estimatedLeftVBottom.Y;
            var probeLeftOffset = -left_estimatedLeftH.X;
            var probeRightOffset = -right_estimatedLeftH.X;

            _loggerService.LogInfo("Result", "XRightProbeOffset: " + probeRightOffset);
            _loggerService.LogInfo("Result", "XLeftProbeOffset: " + probeLeftOffset);
            _loggerService.LogInfo("Result", "YForwardProbeOffset: " + probeForwardOffset);
            _loggerService.LogInfo("Result", "YBackwardsProbeOffset: " + probeBackwardsOffset);
            _loggerService.LogInfo("Result", "ZProbeOffset: " + probeHeightOffset);
        }

        public string GetName()
        {
            return "CalibrateProbeOffsets";
        }
    }
}
