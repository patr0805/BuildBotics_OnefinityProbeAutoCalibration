using OnefinityProbeAutoCalibration.Configuration;
using OnefinityProbeAutoCalibration.Models;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration.Services
{
    public class CalibrationProceduresService : ICalibrationProceduresService
    { 

        public async Task DoFrontSideCalibration(decimal stockWidth, decimal stockLength)
        {
            throw new NotImplementedException();
            /*
            _loggerService.LogInfo("Program", "Calibrate z");
            _loggerService.LogInfo("Program", "Please navigate to a fitting place for calibrating z");
            _loggerService.ReadLine();
            await _onefinityService.ZeroAxis(true, true, true);
            await _subProcedureService.ProbeTowards(null, null, _configuration.ZCalibrationHitLimit, _configuration.ProbeSpeedF);
            await _onefinityService.ZeroAxis(false, false, true);
            await _subProcedureService.MoveTo(null, null, _configuration.ZSafeHeight, _configuration.ProbeFastSpeedF);

            _loggerService.LogInfo("Program", "Please navigate to first (left top) square");
            _loggerService.ReadLine();

            var startPosition = _onefinityService.GetCurrentRelativePosition();
            _loggerService.LogInfo("Program", "Probing edge");
            await _subProcedureService.ProbeTowards(null, null, _configuration.ZCalibrationHitLimit, _configuration.ProbeSpeedF);
            var squareZ = _onefinityService.GetCurrentRelativePosition();

            await _subProcedureService.MoveTo(null, null, squareZ.Z + 3, _configuration.ProbeFastSpeedF);

            await RoughlyProbeForEdge(true, false, 0);
            var leftTopEdge = await FineProbeForEdge(true, true, squareZ.Z + 3);

            var estimatedLeftBottomY = leftTopEdge.Y - stockWidth + _configuration.CalibrationCubeSideWidth * 2;
            await _subProcedureService.MoveTo(startPosition.X, estimatedLeftBottomY, _configuration.ZSafeHeight, _configuration.ProbeFastSpeedF);
            await RoughlyProbeForEdge(true, false, 0);
            var leftBottomEdge = await FineProbeForEdge(true, true, 0);

            await _subProcedureService.MoveTo(null, null, _configuration.ZSafeHeight, _configuration.ProbeFastSpeedF);
            await _subProcedureService.MoveTo(startPosition.X, startPosition.Y, _configuration.ZSafeHeight, _configuration.ProbeFastSpeedF);
            await _subProcedureService.MoveTo(null, null, squareZ.Z + 3, _configuration.ProbeFastSpeedF);

            await RoughlyProbeForEdge(false, true, 0);
            var topLeftEdge = await FineProbeForEdge(false, false, squareZ.Z + 3);
            await _subProcedureService.MoveTo(null, null, _configuration.ZSafeHeight, _configuration.ProbeFastSpeedF);

            var estimatedTopRightX = leftTopEdge.X - stockLength - _configuration.CalibrationCubeSideWidth * 2;
            await _subProcedureService.MoveTo(estimatedTopRightX, leftTopEdge.Y - _configuration.CalibrationCubeSideWidth, _configuration.ZSafeHeight, _configuration.ProbeFastSpeedF);
            await RoughlyProbeForEdge(false, true, 0);

            var topRightTopEdge = await FineProbeForEdge(false, false, 0);
            await _subProcedureService.MoveTo(null, null, _configuration.ZSafeHeight, _configuration.ProbeFastSpeedF);

            // Offset points 
            topLeftEdge = _mathService.OffsetPoint(topLeftEdge, 0, _configuration.BallWidth, 0);
            topRightTopEdge = _mathService.OffsetPoint(topLeftEdge, 0, _configuration.BallWidth, 0);
            leftBottomEdge = _mathService.OffsetPoint(leftBottomEdge, _configuration.BallWidth, 0, 0);
            leftTopEdge = _mathService.OffsetPoint(leftBottomEdge, _configuration.BallWidth, 0, 0);

            var topLine = _mathService.CalculateLineFromPoints(topLeftEdge, topRightTopEdge);
            var leftLine = _mathService.CalculateLineFromPoints(leftTopEdge, leftBottomEdge);
            var topLeftCorner = _mathService.CalculateLineIntersection(topLine, leftLine);

            await _subProcedureService.ProbeTowards(null, null, _configuration.ZCalibrationHitLimit, _configuration.ProbeSpeedF);
            var leftSquareZ = _onefinityService.GetCurrentRelativePosition();
            var estimatedLeftSquareEdge = new Point2D(leftEdge.X + _configuration.BallWidth, leftBottomEdge.Y + _configuration.BallWidth, leftSquareZ.Z);
            await _subProcedureService.MoveTo(estimatedLeftSquareEdge.X + _configuration.CalibrationCubeSideWidth / 3, estimatedLeftSquareEdge.Y + _configuration.CalibrationCubeSideWidth / 3, estimatedLeftSquareEdge.Z - 1, _configuration.ProbeFastSpeedF);
            var squareHitV = await FineProbeForEdge(false, true, estimatedLeftSquareEdge.Z - 1);
            await _subProcedureService.MoveTo(estimatedLeftSquareEdge.X + _configuration.CalibrationCubeSideWidth / 3, estimatedLeftSquareEdge.Y + _configuration.CalibrationCubeSideWidth / 3, estimatedLeftSquareEdge.Z - 1, _configuration.ProbeFastSpeedF);
            var squareHitH = await FineProbeForEdge(true, true, estimatedLeftSquareEdge.Z - 1);

            */
        }


        public async Task CalibrateProbeOffsets(decimal stockWidth, decimal stockLength, decimal stockHeight)
        {
            
        }
    }
}
