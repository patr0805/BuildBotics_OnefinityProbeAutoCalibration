using OnefinityProbeAutoCalibration.Configuration;
using OnefinityProbeAutoCalibration.Models;
using OnefinityProbeAutoCalibration.Services;
using System;
using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration.CalibrationProcedures
{
    public class DoTopCalibrationUsingHolesProcedure : ICalibrationProcedure
    {
        private ILoggerService _loggerService;
        private ISubProcedureService _subProcedureService;
        private IMathService _mathService;
        private IGeometryService _geometryService;
        private IOnefinityService _onefinityService;
        private AppConfiguration _configuration;
        public DoTopCalibrationUsingHolesProcedure(ILoggerService loggerService,
            ISubProcedureService subProcedureService,
            IMathService mathService,
            IOnefinityService onefinityService,
            IGeometryService geometryService,
            AppConfiguration configuration)
        {
            _loggerService = loggerService;
            _subProcedureService = subProcedureService;
            _mathService = mathService;
            _configuration = configuration;
            _geometryService = geometryService;
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
            await _subProcedureService.CalibrateCenter(true); // Calibrate twice to improve acuracy
            await _subProcedureService.CalibrateCenterVH(true); // Calibrate third time to improve acuracy
            var leftHoleCenter = await _subProcedureService.CalibrateCenterVH(true);
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            _loggerService.LogInfo("Program", "Please navigate to second (right) hole");
            _loggerService.ReadLine();
            _loggerService.LogInfo("Program", "Probing center");
            await _subProcedureService.MoveTo(null, null, leftHoleCenter.Z, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.CalibrateCenter(true); // Calibrate twice to improve acuracy
            await _subProcedureService.CalibrateCenterVH(true); // Calibrate third time to improve acuracy
            var rightHoleCenter = await _subProcedureService.CalibrateCenterVH(true);
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            var leftHoleCenterWithoutZ = new Point2D(leftHoleCenter.X, leftHoleCenter.Y, 0);
            var rightHoleCenterWithoutZ = new Point2D(rightHoleCenter.X, rightHoleCenter.Y, 0);
            var angleBetweenPoints = _mathService.AngleBetweenPoints(leftHoleCenterWithoutZ, rightHoleCenterWithoutZ);
            var rotatedPoint = _geometryService.RotatePoint(rightHoleCenterWithoutZ, leftHoleCenterWithoutZ, -angleBetweenPoints);
            var leftCorner = new Point2D(rotatedPoint.X - _configuration.CalibrationConfig.DoTopCalibrationUsingHoles.SecondHoleX, rotatedPoint.Y - _configuration.CalibrationConfig.DoTopCalibrationUsingHoles.SecondHoleY, 0);
            await _subProcedureService.MoveTo(leftCorner.X, leftCorner.Y, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            var stockEdgeVeriticalOffset = Math.Abs(rightHoleCenterWithoutZ.Y - leftHoleCenterWithoutZ.Y);
            var stockAngledTowardsRight = rightHoleCenterWithoutZ.Y <= leftHoleCenterWithoutZ.Y;
            _loggerService.LogInfo("Result", "Stock width: " + stockWidth);
            _loggerService.LogInfo("Result", "Stock length: " + stockLength);
            _loggerService.LogInfo("Result", "Angle: " + angleBetweenPoints);
            _loggerService.LogInfo("Result", "Tilt distance between left and right corner: " + stockEdgeVeriticalOffset);
            _loggerService.LogInfo("Result", "Stock titled to the: " + (stockAngledTowardsRight ? "Right" : "Left"));
            await _onefinityService.SetZeroOffset(leftCorner);
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.MoveTo(0, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSuperSpeedF);
        }

        public string GetName()
        {
            return "DoTopCalibrationUsingHoles";
        }
    }
}
