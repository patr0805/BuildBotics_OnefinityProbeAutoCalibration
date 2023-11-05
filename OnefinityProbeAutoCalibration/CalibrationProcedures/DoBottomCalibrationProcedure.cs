using OnefinityProbeAutoCalibration.Configuration;
using OnefinityProbeAutoCalibration.Models;
using OnefinityProbeAutoCalibration.Services;
using System;
using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration.CalibrationProcedures
{
    public class DoBottomCalibrationProcedure : ICalibrationProcedure
    {
        private ILoggerService _loggerService;
        private ISubProcedureService _subProcedureService;
        private IMathService _mathService;
        private IOnefinityService _onefinityService;
        private AppConfiguration _configuration;
        public DoBottomCalibrationProcedure(ILoggerService loggerService,
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
            _loggerService.LogInfo("Program", "Calibrate z");
            _loggerService.LogInfo("Program", "Please navigate to the top of the left calibrationblock");
            _loggerService.ReadLine();
            await _onefinityService.ZeroAxis(true, true, true);
            await _subProcedureService.ProbeTowards(null, null, _configuration.Config.ZCalibrationHitLimit, _configuration.Config.ProbeSpeedF);
            await _onefinityService.ZeroAxis(false, false, true);
            await _subProcedureService.MoveTo(null, null, _configuration.Config.CalibrationSideOffset, _configuration.Config.ProbeFastSpeedF);
            await _onefinityService.ZeroAxis(false, false, true);
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            await _subProcedureService.RoughlyProbeForEdge(true, false, 0);
            var estimatedLeftH = await _subProcedureService.FineProbeForEdge(true, true);

            await _subProcedureService.MoveTo(estimatedLeftH.X + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(false, false, 0);
            var estimatedLeftVBottom = await _subProcedureService.FineProbeForEdge(false, true);

            await _subProcedureService.MoveTo(estimatedLeftH.X + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, estimatedLeftVBottom.Y + stockWidth - _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(false, true, 0);
            var estimatedLeftVTop = await _subProcedureService.FineProbeForEdge(false, false);

            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.MoveTo(estimatedLeftH.X + stockLength - _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(true, true, 0);
            var estimatedRightH = await _subProcedureService.FineProbeForEdge(true, false);

            await _subProcedureService.MoveTo(estimatedRightH.X - _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(false, false, 0);
            var estimatedRightVBottom = await _subProcedureService.FineProbeForEdge(false, true);

            await _subProcedureService.MoveTo(estimatedRightH.X - _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(false, true, 0);
            var estimatedRightVTop = await _subProcedureService.FineProbeForEdge(false, false);

            // Measure bottom left corner
            await _subProcedureService.MoveTo(estimatedLeftVBottom.X, estimatedLeftVBottom.Y + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(true, false, 0);
            var estimatedLeftBottomCornerH = await _subProcedureService.FineProbeForEdge(true, true);

            await _subProcedureService.MoveTo(estimatedLeftVBottom.X, estimatedLeftVBottom.Y + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing * 3, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(true, false, 0);
            var estimatedLeftBottomCorner2H = await _subProcedureService.FineProbeForEdge(true, true);

            // Measure bottom right corner
            await _subProcedureService.MoveTo(estimatedRightVBottom.X, estimatedRightVBottom.Y + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(true, true, 0);
            var estimatedRightBottomCornerH = await _subProcedureService.FineProbeForEdge(true, false);

            await _subProcedureService.MoveTo(estimatedRightVBottom.X, estimatedRightVBottom.Y + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing * 3, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(true, true, 0);
            var estimatedRightBottomCorner2H = await _subProcedureService.FineProbeForEdge(true, false);

            // Offset points by probe ball size
            estimatedLeftVTop = _mathService.OffsetPoint(estimatedLeftVTop, 0, -_configuration.CalibrationConfig.ProbeConfig.YBackwardsProbeOffset, 0);
            estimatedRightVTop = _mathService.OffsetPoint(estimatedRightVTop, 0, -_configuration.CalibrationConfig.ProbeConfig.YBackwardsProbeOffset, 0);
            estimatedLeftVBottom = _mathService.OffsetPoint(estimatedLeftVBottom, 0, _configuration.CalibrationConfig.ProbeConfig.YForwardProbeOffset, 0);
            estimatedRightVBottom = _mathService.OffsetPoint(estimatedRightVBottom, 0, _configuration.CalibrationConfig.ProbeConfig.YForwardProbeOffset, 0);
            estimatedLeftBottomCornerH = _mathService.OffsetPoint(estimatedLeftBottomCornerH, _configuration.CalibrationConfig.ProbeConfig.XRightProbeOffset, 0, 0);
            estimatedLeftBottomCorner2H = _mathService.OffsetPoint(estimatedLeftBottomCorner2H, _configuration.CalibrationConfig.ProbeConfig.XRightProbeOffset, 0, 0);
            estimatedRightBottomCornerH = _mathService.OffsetPoint(estimatedRightBottomCornerH, -_configuration.CalibrationConfig.ProbeConfig.XLeftProbeOffset, 0, 0);
            estimatedRightBottomCorner2H = _mathService.OffsetPoint(estimatedRightBottomCorner2H, -_configuration.CalibrationConfig.ProbeConfig.XLeftProbeOffset, 0, 0);

            // Calculate adjustments
            var topLine = _mathService.CalculateLineFromPoints(estimatedLeftVTop, estimatedRightVTop);
            var bottomLine = _mathService.CalculateLineFromPoints(estimatedLeftVBottom, estimatedRightVBottom);
            var leftSideLine = _mathService.CalculateLineFromPoints(estimatedLeftBottomCornerH, estimatedLeftBottomCorner2H);
            var rightSideLine = _mathService.CalculateLineFromPoints(estimatedRightBottomCornerH, estimatedRightBottomCorner2H);
            var leftTopCorner = _mathService.CalculateLineIntersection(topLine, leftSideLine);
            var rightTopCorner = _mathService.CalculateLineIntersection(topLine, rightSideLine);

            // Resize rect to match stockLength, stockWidth
            // TODO

            // Calculate stock details
            var leftBottomCorner = _mathService.CalculateLineIntersection(bottomLine, leftSideLine);
            var rightBottomCorner = _mathService.CalculateLineIntersection(bottomLine, rightSideLine);
            // var centerPoint = new Point2D((leftTopCorner.X + leftBottomCorner.X) / 2, (leftTopCorner.Y + leftBottomCorner.Y) / 2, rightTopCorner.Z);
            var newStockLength = _mathService.CalculatePoint2DDistance(leftBottomCorner, rightBottomCorner);
            var newStockWidth = _mathService.CalculatePoint2DDistance(leftBottomCorner, _mathService.CalculateLineIntersection(topLine, leftSideLine));
           
            var stockEdgeVeriticalOffset = Math.Abs(rightBottomCorner.Y - leftBottomCorner.Y);
            var stockAngledTowardsRight = rightBottomCorner.Y <= leftBottomCorner.Y;
            var angle = _mathService.ConvertRadiansToDegrees(Math.Asin(Convert.ToDouble(stockEdgeVeriticalOffset / stockLength)));

            var stockWidthOffset = newStockWidth - stockWidth;
            var stockLengthOffset = newStockLength - stockLength;
            _loggerService.LogInfo("Result", "Stock width: " + newStockWidth);
            _loggerService.LogInfo("Result", "Stock height: " + stockHeight);
            _loggerService.LogInfo("Result", "Stock length: " + newStockLength);
            _loggerService.LogInfo("Result", "Stock width offset: " + stockWidthOffset);
            _loggerService.LogInfo("Result", "Stock width offset: " + stockLengthOffset);
            _loggerService.LogInfo("Result", "Angle: " + angle);
            _loggerService.LogInfo("Result", "Tilt distance between left and right corner: " + stockEdgeVeriticalOffset);
            _loggerService.LogInfo("Result", "Stock titled to the: " + (stockAngledTowardsRight ? "Right" : "Left"));
            await _onefinityService.SetZeroOffset(leftBottomCorner);
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.MoveTo(0, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
        }

        public string GetName()
        {
            return "DoBottomCalibration";
        }
    }
}
