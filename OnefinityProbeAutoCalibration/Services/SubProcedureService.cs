using System;
using System.Threading.Tasks;
using System.Threading;
using OnefinityProbeAutoCalibration.Models;
using OnefinityProbeAutoCalibration.Configuration;

namespace OnefinityProbeAutoCalibration.Services
{
    public class SubProcedureService : ISubProcedureService
    {
        private IOnefinityService _onefinityService;
        private IEventService _eventService;
        private IMathService _mathService;
        private IStateService _stateService;
        private ILoggerService _loggerService;
        private AppConfiguration _configuration;
        private IGeometryService _geometryService;

        public SubProcedureService(IOnefinityService onefinityService,
            IEventService eventService,
            IMathService mathService,
            IStateService stateService,
            ILoggerService loggerService,
            AppConfiguration appConfiguration,
            IGeometryService geometryService)
        {
            _onefinityService = onefinityService;
            _eventService = eventService;
            _mathService = mathService;
            _stateService = stateService;
            _loggerService = loggerService;
            _configuration = appConfiguration;
            _geometryService = geometryService;
        }

        public Task MoveTo(decimal? x, decimal? y, decimal? z, int? f)
        {
            var currentPosition = _onefinityService.GetCurrentRelativePosition();
            if ((!x.HasValue || x == currentPosition.X) && (!y.HasValue || y == currentPosition.Y) && (!z.HasValue || z == currentPosition.Z))
            {
                return Task.CompletedTask;
            }

            var absoluteX = x != null ? _mathService.ConvertRelativeToAbsolute(x.Value, _stateService.GetMachineState().offset_x) : (decimal?)null;
            if (x != null && (absoluteX  < 0 || absoluteX >= _configuration.Config.MachineWidth))
            {
                throw new Exception("FATAL ERROR, machine out of bounds!");
            }

            var absoluteY = y != null ? _mathService.ConvertRelativeToAbsolute(y.Value, _stateService.GetMachineState().offset_y) : (decimal?)null;
            if (y != null && (absoluteY < 0 || absoluteY >= _configuration.Config.MachineLength))
            {
                throw new Exception("FATAL ERROR, machine out of bounds!");
            }

            var xString = x != null ? $"X{string.Format("{0:0.###}", x.Value).Replace(",", ".")}" : "";
            var yString = y != null ? $"Y{string.Format("{0:0.###}", y.Value).Replace(",", ".")}" : "";
            var fString = f != null ? $"F{string.Format("{0:0}", f.Value).Replace(",", ".")}" : "";
            var combinedParts = (xString + " " + yString + " " + fString).Replace("  ", " ").Trim();
            var commandToSend = "";
            if (x != null || y != null)
            {
                commandToSend += $"G90 G1 {combinedParts}";
            }

            if (z != null)
            {
                var zString = $"Z{string.Format("{0:0.###}", z.Value).Replace(",", ".")}";
                var combinedParts2 = (zString + " " + fString).Replace("  ", " ").Trim();
                var zCommand = $"G90 G1 {combinedParts2}";
                commandToSend = commandToSend == "" ? zCommand : commandToSend + Environment.NewLine + zCommand;
            }

            var id = _stateService.GetNextId();
            var pass = new SemaphoreSlim(0, 1);
            _eventService.SubscribeMoveTo(id.ToString(), x, y, z, () => pass.Release());
            _onefinityService.ScheduleSend(id, commandToSend);
            return pass.WaitAsync();
        }

        public async Task<bool> ProbeTowards(decimal? x, decimal? y, decimal? z, int? f, bool throwExceptionOnNonHit = true)
        {
            var currentPosition = _onefinityService.GetCurrentRelativePosition();
            if ((!x.HasValue || x == currentPosition.X) && (!y.HasValue || y == currentPosition.Y) && (!z.HasValue || z == currentPosition.Z))
            {
                return false;
            }

            var xString = x != null ? $" X{string.Format("{0:0.###}", x.Value).Replace(",", ".")}" : "";
            var yString = y != null ? $" Y{string.Format("{0:0.###}", y.Value).Replace(",", ".")}" : "";
            var fString = f != null ? $" F{string.Format("{0:0}", f.Value).Replace(",", ".")}" : "";
            var zString = z != null ? $" Z{string.Format("{0:0.###}", z.Value).Replace(",", ".")}" : "";
            var combinedParts = (xString + yString + zString + fString).Replace("  ", " ").Trim();
            var commandToSend = "";
            if (x != null || y != null || z != null)
            {
                commandToSend += $"G90 G38.3 {combinedParts}";
            }

            var id = _stateService.GetNextId();
            var pass = new SemaphoreSlim(0, 1);
            _eventService.SubscribeMoveTo(id.ToString(), x, y, z, () => pass.Release());
            _onefinityService.ScheduleSend(id, commandToSend);
            await pass.WaitAsync();
            if (throwExceptionOnNonHit)
            {
                Thread.Sleep(1000);
            }

            if (throwExceptionOnNonHit && _mathService.RelativeCoordinateEquals(_stateService.GetMachineState(), x, y, z))
            {
                throw new Exception("Probe did not hit when it was expected to! Aborting!");
            }

            if (_mathService.RelativeCoordinateEquals(_stateService.GetMachineState(), x, y, z))
            {
                // There is a bug where the system only discovers that its stuck till stop is triggered.. 
                _onefinityService.Stop();
            }

            return !_mathService.RelativeCoordinateEquals(_stateService.GetMachineState(), x, y, z);
        }

        public async Task<Point2D> CalibrateCenter(bool skipZ = false)
        {
            _loggerService.LogInfo("CalibrateCenter", "Calibrating..");
            var userPlacedPosition = _onefinityService.GetCurrentRelativePosition();
            var zHitPoint = Math.Min(userPlacedPosition.Z, _configuration.Config.ZHitSmallHeight);
            if (!skipZ && await ProbeTowards(null, null, zHitPoint, _configuration.Config.ProbeSpeedF, throwExceptionOnNonHit: false))
            {
                throw new Exception("Unexpected hit!");
            }

            var safePlaceInHole = _onefinityService.GetCurrentRelativePosition();
            await ProbeTowards(safePlaceInHole.X + _configuration.Config.CalibrationHoleDiameter + _configuration.Config.CalibrationExtraLimit, safePlaceInHole.Y, skipZ ? (decimal?)null : zHitPoint, _configuration.Config.ProbeSpeedF);

            var rightHitPoint = _onefinityService.GetCurrentRelativePosition();
            await MoveTo(safePlaceInHole.X, safePlaceInHole.Y, skipZ ? (decimal?)null : zHitPoint, _configuration.Config.ProbeFastSpeedF);
            await ProbeTowards(safePlaceInHole.X - (_configuration.Config.CalibrationHoleDiameter + _configuration.Config.CalibrationExtraLimit), safePlaceInHole.Y + (_configuration.Config.CalibrationHoleDiameter + _configuration.Config.CalibrationExtraLimit), skipZ ? (decimal?)null : zHitPoint, _configuration.Config.ProbeSpeedF);

            var diagonalUpHitPoint = _onefinityService.GetCurrentRelativePosition();
            await MoveTo(safePlaceInHole.X, safePlaceInHole.Y, skipZ ? (decimal?)null : zHitPoint, _configuration.Config.ProbeFastSpeedF);
            await ProbeTowards(safePlaceInHole.X - (_configuration.Config.CalibrationHoleDiameter + _configuration.Config.CalibrationExtraLimit), safePlaceInHole.Y - (_configuration.Config.CalibrationHoleDiameter + _configuration.Config.CalibrationExtraLimit), skipZ ? (decimal?)null : zHitPoint, _configuration.Config.ProbeSpeedF);

            var diagonalBottomHitPoint = _onefinityService.GetCurrentRelativePosition();

            // Offset measurements by probe ball offsets
            rightHitPoint.X += _configuration.CalibrationConfig.ProbeConfig.XRightProbeOffset;
            diagonalUpHitPoint.X -= _configuration.CalibrationConfig.ProbeConfig.XLeftProbeOffset;
            diagonalUpHitPoint.Y += _configuration.CalibrationConfig.ProbeConfig.YForwardProbeOffset;
            diagonalBottomHitPoint.X -= _configuration.CalibrationConfig.ProbeConfig.XLeftProbeOffset;
            diagonalBottomHitPoint.Y -= _configuration.CalibrationConfig.ProbeConfig.YBackwardsProbeOffset;

            var centerPoint = _geometryService.FindCenterOfCircleGivenThreePoints(rightHitPoint, diagonalUpHitPoint, diagonalBottomHitPoint);
            await MoveTo(centerPoint.X, centerPoint.Y, skipZ ? (decimal?)null : zHitPoint, _configuration.Config.ProbeFastSpeedF);
            return centerPoint;
        }
        public async Task<Point2D> CalibrateCenterVH(bool skipZ = false)
        {
            _loggerService.LogInfo("CalibrateCenter", "Calibrating..");
            var userPlacedPosition = _onefinityService.GetCurrentRelativePosition();
            var zHitPoint = Math.Min(userPlacedPosition.Z, _configuration.Config.ZHitSmallHeight);
            if (!skipZ && await ProbeTowards(null, null, zHitPoint, _configuration.Config.ProbeSpeedF, throwExceptionOnNonHit: false))
            {
                throw new Exception("Unexpected hit!");
            }

            var safePlaceInHole = _onefinityService.GetCurrentRelativePosition();
            await ProbeTowards(safePlaceInHole.X + _configuration.Config.CalibrationHoleDiameter + _configuration.Config.CalibrationExtraLimit, safePlaceInHole.Y, skipZ ? (decimal?)null : zHitPoint, _configuration.Config.ProbeSpeedF);

            var rightHitPoint = _onefinityService.GetCurrentRelativePosition();
            await MoveTo(safePlaceInHole.X, safePlaceInHole.Y, skipZ ? (decimal?)null : zHitPoint, _configuration.Config.ProbeFastSpeedF);
            await ProbeTowards(safePlaceInHole.X - (_configuration.Config.CalibrationHoleDiameter + _configuration.Config.CalibrationExtraLimit), safePlaceInHole.Y, skipZ ? (decimal?)null : zHitPoint, _configuration.Config.ProbeSpeedF);

            var leftHitPoint = _onefinityService.GetCurrentRelativePosition();
            await MoveTo(safePlaceInHole.X, safePlaceInHole.Y, skipZ ? (decimal?)null : zHitPoint, _configuration.Config.ProbeFastSpeedF);
            await ProbeTowards(safePlaceInHole.X, safePlaceInHole.Y + (_configuration.Config.CalibrationHoleDiameter + _configuration.Config.CalibrationExtraLimit), skipZ ? (decimal?)null : zHitPoint, _configuration.Config.ProbeSpeedF);

            var topHitPoint = _onefinityService.GetCurrentRelativePosition();

            // Offset measurements by probe ball offsets
            rightHitPoint.X += _configuration.CalibrationConfig.ProbeConfig.XRightProbeOffset;
            leftHitPoint.X -= _configuration.CalibrationConfig.ProbeConfig.XLeftProbeOffset;
            topHitPoint.Y += _configuration.CalibrationConfig.ProbeConfig.YForwardProbeOffset;

            var centerPoint = _geometryService.FindCenterOfCircleGivenThreePoints(rightHitPoint, leftHitPoint, topHitPoint);
            await MoveTo(centerPoint.X, centerPoint.Y, skipZ ? (decimal?)null : zHitPoint, _configuration.Config.ProbeFastSpeedF);
            return centerPoint;
        }

        public async Task<Point2D> RoughlyProbeForEdge(bool horizontal, bool rightOrUp, decimal stockTopZ)
        {
            var currentRelativePosition = _onefinityService.GetCurrentRelativePosition();
            var targetPos = horizontal ? currentRelativePosition.X : currentRelativePosition.Y;

            do
            {
                targetPos += rightOrUp ? _configuration.Config.AutoLocateAndMeasureStockJumpDistance : -_configuration.Config.AutoLocateAndMeasureStockJumpDistance;
                await MoveTo(horizontal ? targetPos : (decimal?)null, !horizontal ? targetPos : (decimal?)null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
                if (!await ProbeTowards(null, null, _configuration.Config.ZHitSmallHeight, _configuration.Config.ProbeSpeedRoughF, false))
                {
                    break;
                }

                await MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            }
            while (true);
            //await _subProcedureService.MoveTo(null, null, _configuration.ZSafeHeight, _configuration.ProbeFastSpeedF);
            return _onefinityService.GetCurrentRelativePosition();
        }

        public async Task<Point2D> FineProbeForEdge(bool horizontal, bool rightOrUp, bool skipRetreat = false)
        {
            var currentRelativePosition = _onefinityService.GetCurrentRelativePosition();
            if (!skipRetreat)
            {
                await MoveTo(null, null, _configuration.Config.ZHitHeight, _configuration.Config.ProbeFastSpeedF);
            }

            var offset = rightOrUp ? _configuration.Config.AutoLocateAndMeasureStockJumpDistance : -_configuration.Config.AutoLocateAndMeasureStockJumpDistance;
            if (await ProbeTowards(horizontal ? offset : (decimal?)null, !horizontal ? offset : (decimal?)null, null, _configuration.Config.ProbeSpeedF, true))
            {
                var hitPoint = _onefinityService.GetCurrentRelativePosition();
                if (!skipRetreat)
                {
                    await MoveTo(currentRelativePosition.X, currentRelativePosition.Y, null, _configuration.Config.ProbeFastSpeedF);
                    await MoveTo(currentRelativePosition.X, currentRelativePosition.Y, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
                }

                return hitPoint;
            }

            throw new Exception("Fine probing failed!");
        }
    }
}
