using OnefinityProbeAutoCalibration.Configuration;
using OnefinityProbeAutoCalibration.Models;
using OnefinityProbeAutoCalibration.Services;
using System;
using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration.CalibrationProcedures
{
    public class DoBottomCalibrationUsingHolesProcedure : ICalibrationProcedure
    {
        private ILoggerService _loggerService;
        private ISubProcedureService _subProcedureService;
        private IMathService _mathService;
        private IOnefinityService _onefinityService;
        private AppConfiguration _configuration;
        public DoBottomCalibrationUsingHolesProcedure(ILoggerService loggerService,
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
            _loggerService.LogInfo("Program", "Please navigate to a fitting place for calibrating z");
            _loggerService.ReadLine();
            await _onefinityService.ZeroAxis(true, true, true);
            await _subProcedureService.ProbeTowards(null, null, _configuration.Config.ZCalibrationHitLimit, _configuration.Config.ProbeSpeedF);
            await _onefinityService.ZeroAxis(false, false, true);
            await _subProcedureService.MoveTo(null, null, _configuration.CalibrationConfig.ProbeConfig.ZProbeOffset, _configuration.Config.ProbeFastSpeedF);
            await _onefinityService.ZeroAxis(false, false, true);
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            _loggerService.LogInfo("Program", "Please navigate to first (left) hole");
            _loggerService.ReadLine();
            _loggerService.LogInfo("Program", "Probing center");
            await _subProcedureService.CalibrateCenter(); // Calibrate twice to improve acuracy
            await _subProcedureService.CalibrateCenter(); // Calibrate third time to improve acuracy
            var leftHoleCenter = await _subProcedureService.CalibrateCenter();
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            _loggerService.LogInfo("Program", "Please navigate to second (right) hole");
            _loggerService.ReadLine();
            _loggerService.LogInfo("Program", "Probing center");
            await _subProcedureService.MoveTo(null, null, leftHoleCenter.Z, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.CalibrateCenter(); // Calibrate twice to improve acuracy
            await _subProcedureService.CalibrateCenter(); // Calibrate third time to improve acuracy
            var rightHoleCenter = await _subProcedureService.CalibrateCenter();
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            var leftHoleCenterWithoutZ = new Point2D(leftHoleCenter.X, leftHoleCenter.Y, 0);
            var rightHoleCenterWithoutZ = new Point2D(rightHoleCenter.X, rightHoleCenter.Y, 0);
            var centersLine = _mathService.CalculateLineFromPoints(leftHoleCenterWithoutZ, rightHoleCenterWithoutZ);
            var centersLineUnitVector = _mathService.GetUnitVector(leftHoleCenterWithoutZ, rightHoleCenterWithoutZ);
            var centerPoint = new Point2D((leftHoleCenterWithoutZ.X + rightHoleCenterWithoutZ.X) / 2, (leftHoleCenterWithoutZ.Y + rightHoleCenterWithoutZ.Y) / 2, leftHoleCenterWithoutZ.Z);
            var leftEdge = _mathService.OffsetPoint(centerPoint, -centersLineUnitVector.X * stockLength / 2, -centersLineUnitVector.Y * stockLength / 2, 0);
            var rightEdge = _mathService.OffsetPoint(centerPoint, centersLineUnitVector.X * stockLength / 2, centersLineUnitVector.Y * stockLength / 2, 0);
            var leftSideLine = _mathService.CalculateOrthogonalLine(centersLine, leftEdge);
            var rightSideLine = _mathService.CalculateOrthogonalLine(centersLine, rightEdge);
            var topLine = _mathService.OffsetLine(centersLine, stockWidth / 2);
            var bottomLine = _mathService.OffsetLine(centersLine, -stockWidth / 2);

            var leftBottomCorner = _mathService.CalculateLineIntersection(bottomLine, leftSideLine);
            var rightBottomCorner = _mathService.CalculateLineIntersection(bottomLine, rightSideLine);
            var leftTopCorner = _mathService.CalculateLineIntersection(topLine, leftSideLine);
            var stockEdgeVeriticalOffset = Math.Abs(rightBottomCorner.Y - leftBottomCorner.Y);
            var stockAngledTowardsRight = rightBottomCorner.Y <= leftBottomCorner.Y;
            var angle = _mathService.ConvertRadiansToDegrees(Math.Asin(Convert.ToDouble(stockEdgeVeriticalOffset / stockLength)));
            _loggerService.LogInfo("Result", "Stock width: " + stockWidth);
            _loggerService.LogInfo("Result", "Stock length: " + stockLength);
            _loggerService.LogInfo("Result", "Angle: " + angle);
            _loggerService.LogInfo("Result", "Tilt distance between left and right corner: " + stockEdgeVeriticalOffset);
            _loggerService.LogInfo("Result", "Stock titled to the: " + (stockAngledTowardsRight ? "Right" : "Left"));
            await _onefinityService.SetZeroOffset(leftTopCorner);
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.MoveTo(0, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
        }

        public string GetName()
        {
            return "DoBottomCalibrationUsingHoles";
        }
    }
}
