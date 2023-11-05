using Newtonsoft.Json.Linq;
using OnefinityProbeAutoCalibration.Configuration;
using OnefinityProbeAutoCalibration.Models;
using OnefinityProbeAutoCalibration.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration.CalibrationProcedures
{
    public class AutoLocateAndMeasureStockProcedure : ICalibrationProcedure
    {
        private IStateService _stateService;
        private ILoggerService _loggerService;
        private ISubProcedureService _subProcedureService;
        private IMathService _mathService;
        private IOnefinityService _onefinityService;
        private AppConfiguration _configuration;
        public AutoLocateAndMeasureStockProcedure(ILoggerService loggerService,
            IStateService stateService,
            ISubProcedureService subProcedureService,
            IMathService mathService,
            IOnefinityService onefinityService,
            AppConfiguration configuration)
        {
            _loggerService = loggerService;
            _stateService = stateService;
            _subProcedureService = subProcedureService;
            _mathService = mathService;
            _configuration = configuration;
            _onefinityService = onefinityService;
        }

        public async Task Apply(decimal stockWidth, decimal stockLength, decimal stockHeight)
        {
            _loggerService.LogInfo("Program", "Please navigate to roughly the left corner of the stock");
            _loggerService.ReadLine();

            _loggerService.LogInfo("Program", "Roughly measuring stock");
            await _onefinityService.ZeroAxis(true, true, true);
            await _subProcedureService.ProbeTowards(null, null, _configuration.Config.ZCalibrationHitLimit, _configuration.Config.ProbeSpeedF);
            await _onefinityService.ZeroAxis(false, false, true);
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            // Measure height
            Console.WriteLine("Please navigate probe to a coordinate to measure bed z height");
            Console.ReadLine();

            if (!await _subProcedureService.ProbeTowards(null, null, _mathService.ConvertAbsolutePositionToRelative(-130, _stateService.GetMachineState().offset_z), _configuration.Config.ProbeSpeedF, false))
            {
                throw new Exception("Probe did not hit bottom!");
            }

            var bottomPos = _onefinityService.GetCurrentRelativePosition();
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.MoveTo(0, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            var result = await RoughCalibrate(new AutoLocateAutoCalibrationResultModel());
            _loggerService.LogInfo("Program", "Finely measuring stock");
            result = await FineCalibrate(result);
            //result = await CenterSideCalibrate(result);
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            // Calculate adjustments
            var topLine = _mathService.CalculateLineFromPoints(result.EstimatedLeftVTop, result.EstimatedRightVTop);
            var bottomLine = _mathService.CalculateLineFromPoints(result.EstimatedLeftVBottom, result.EstimatedRightVBottom);
            var leftSideLine = _mathService.CalculateLineFromPoints(result.EstimatedLeftBottomCornerH, result.EstimatedLeftBottomCorner2H);
            var rightSideLine = _mathService.CalculateLineFromPoints(result.EstimatedRightBottomCornerH, result.EstimatedRightBottomCorner2H);
            var leftTopCorner = _mathService.CalculateLineIntersection(topLine, leftSideLine);
            var rightTopCorner = _mathService.CalculateLineIntersection(topLine, rightSideLine);

            // Calculate stock details
            var leftBottomCorner = _mathService.CalculateLineIntersection(bottomLine, leftSideLine);
            var rightBottomCorner = _mathService.CalculateLineIntersection(bottomLine, rightSideLine);
            // var centerPoint = new Point2D((leftTopCorner.X + leftBottomCorner.X) / 2, (leftTopCorner.Y + leftBottomCorner.Y) / 2, rightTopCorner.Z);
            stockLength = _mathService.CalculatePoint2DDistance(leftBottomCorner, rightBottomCorner);
            stockWidth = _mathService.CalculatePoint2DDistance(leftBottomCorner, _mathService.CalculateLineIntersection(topLine, leftSideLine));
            stockHeight = Math.Abs(bottomPos.Z + _configuration.CalibrationConfig.ProbeConfig.ZProbeOffset * 2);

            // Disabled until I have a probe for this, a probe that can reach the bottom of the stock
            if (false)
            {
                var offsetmodel = await CalibrateBottomOffset(result, stockHeight);
                PrintBottomOffset(offsetmodel);
            }

            await _onefinityService.SetZeroOffset(leftBottomCorner);
            await _subProcedureService.MoveTo(null, null, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.MoveTo(0, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);

            // Update appsettings.json
            var json = File.ReadAllText("appSettings.json");

            // Parse the JSON string into a JObject
            JObject jsonObj = JObject.Parse(json);

            // Get the StockLength property and update its value
            var stockLengthToken = jsonObj.SelectToken("Config.StockConfig.Length");
            stockLengthToken.Replace(stockLength);

            var stockWidthToken = jsonObj.SelectToken("Config.StockConfig.Width");
            stockWidthToken.Replace(stockWidth);

            var stockHeightToken = jsonObj.SelectToken("Config.StockConfig.Height");
            stockHeightToken.Replace(stockHeight);

            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText("appSettings.json", output);
            File.WriteAllText("../../../appSettings.json", output);

            var stockEdgeVeriticalOffset = Math.Abs(rightBottomCorner.Y - leftBottomCorner.Y);
            var stockAngledTowardsRight = rightBottomCorner.Y <= leftBottomCorner.Y;
            var stockZOffset = _mathService.ConvertRelativeToAbsolute(bottomPos.Z, _stateService.GetMachineState().offset_z);
            var angle = _mathService.ConvertRadiansToDegrees(Math.Asin(Convert.ToDouble(stockEdgeVeriticalOffset / stockLength)));
            _loggerService.LogInfo("Result", "Stock width: " + stockWidth);
            _loggerService.LogInfo("Result", "Stock height: " + stockHeight);
            _loggerService.LogInfo("Result", "Stock length: " + stockLength);
            _loggerService.LogInfo("Result", "Stock z start height: " + stockZOffset);
            _loggerService.LogInfo("Result", "Angle: " + angle);
            _loggerService.LogInfo("Result", "Tilt distance between left and right corner: " + stockEdgeVeriticalOffset);
            _loggerService.LogInfo("Result", "Stock titled to the: " + (stockAngledTowardsRight ? "Right" : "Left"));
        }

        //private Task<AutoLocateAutoCalibrationResultModel> CenterSideCalibrate(AutoLocateAutoCalibrationResultModel result)
        //{
        //    // Calculate adjustments
        //    var topLine = _mathService.CalculateLineFromPoints(result.EstimatedLeftVTop, result.EstimatedRightVTop);
        //    var bottomLine = _mathService.CalculateLineFromPoints(result.EstimatedLeftVBottom, result.EstimatedRightVBottom);
        //    var leftSideLine = _mathService.CalculateLineFromPoints(result.EstimatedLeftBottomCornerH, result.EstimatedLeftBottomCorner2H);
        //    var rightSideLine = _mathService.CalculateLineFromPoints(result.EstimatedRightBottomCornerH, result.EstimatedRightBottomCorner2H);
        //    // Equalize slopes
        //    rightSideLine.A = leftSideLine.A;
        //    topLine.A = bottomLine.A;

        //    var leftTopCorner = _mathService.CalculateLineIntersection(topLine, leftSideLine);
        //    var rightTopCorner = _mathService.CalculateLineIntersection(topLine, rightSideLine);

        //    // Calculate stock details
        //    var leftBottomCorner = _mathService.CalculateLineIntersection(bottomLine, leftSideLine);
        //    var rightBottomCorner = _mathService.CalculateLineIntersection(bottomLine, rightSideLine);

        //    // Get accurate left side
        //    var estimatedLeftSidePoint = new Point2D(leftTopCorner.X + (leftBottomCorner.X - leftTopCorner.X) / 2, leftTopCorner.Y - leftBottomCorner.Y, 0);
        //    var estimatedRightSidePoint = new Point2D(leftTopCorner.X + (leftBottomCorner.X - leftTopCorner.X) / 2, rightTopCorner.Y - rightBottomCorner.Y, 0);
        //    var estimatedTopSidePoint = new Point2D(leftTopCorner.X + Math.Abs(rightTopCorner.X - leftTopCorner.X), rightTopCorner.Y - leftTopCorner.Y, 0);
        //    var estimatedBottomSidePoint = new Point2D(Math.Min(rightTopCorner.X, rightBottomCorner.X) + Math.Abs(rightTopCorner.X - rightBottomCorner.X), rightTopCorner.Y - rightBottomCorner.Y, 0);
        //}

        private async Task<AutoLocateAutoCalibrationResultModel> FineCalibrate(AutoLocateAutoCalibrationResultModel input)
        {
            // Measure bottom left corner
            await _subProcedureService.MoveTo(input.EstimatedLeftVBottom.X, input.EstimatedLeftVBottom.Y + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(true, false, 0);
            input.EstimatedLeftBottomCornerH = await _subProcedureService.FineProbeForEdge(true, true);

            await _subProcedureService.MoveTo(input.EstimatedLeftVBottom.X, input.EstimatedLeftVBottom.Y + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing * 3, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(true, false, 0);
            input.EstimatedLeftBottomCorner2H = await _subProcedureService.FineProbeForEdge(true, true);

            // Measure bottom right corner
            await _subProcedureService.MoveTo(input.EstimatedRightVBottom.X, input.EstimatedRightVBottom.Y + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(true, true, 0);
            input.EstimatedRightBottomCornerH = await _subProcedureService.FineProbeForEdge(true, false);

            await _subProcedureService.MoveTo(input.EstimatedRightVBottom.X, input.EstimatedRightVBottom.Y + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing * 3, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(true, true, 0);
            input.EstimatedRightBottomCorner2H = await _subProcedureService.FineProbeForEdge(true, false);

            // Offset points by probe ball size
            input.EstimatedLeftBottomCornerH = _mathService.OffsetPoint(input.EstimatedLeftBottomCornerH, _configuration.CalibrationConfig.ProbeConfig.XRightProbeOffset, 0, 0);
            input.EstimatedLeftBottomCorner2H = _mathService.OffsetPoint(input.EstimatedLeftBottomCorner2H, _configuration.CalibrationConfig.ProbeConfig.XRightProbeOffset, 0, 0);
            input.EstimatedRightBottomCornerH = _mathService.OffsetPoint(input.EstimatedRightBottomCornerH, -_configuration.CalibrationConfig.ProbeConfig.XLeftProbeOffset, 0, 0);
            input.EstimatedRightBottomCorner2H = _mathService.OffsetPoint(input.EstimatedRightBottomCorner2H, -_configuration.CalibrationConfig.ProbeConfig.XLeftProbeOffset, 0, 0);

            return input;
        }
        private async Task<AutoLocateAutoCalibrationResultModel> RoughCalibrate(AutoLocateAutoCalibrationResultModel input)
        {
            await _subProcedureService.RoughlyProbeForEdge(true, false, 0);
            input.EstimatedLeftH = await _subProcedureService.FineProbeForEdge(true, true);

            await _subProcedureService.MoveTo(input.EstimatedLeftH.X + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(false, false, 0);
            input.EstimatedLeftVBottom = await _subProcedureService.FineProbeForEdge(false, true);

            await _subProcedureService.MoveTo(input.EstimatedLeftH.X + _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(false, true, 0);
            input.EstimatedLeftVTop = await _subProcedureService.FineProbeForEdge(false, false);

            await _subProcedureService.MoveTo(0, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(true, true, 0);
            input.EstimatedRightH = await _subProcedureService.FineProbeForEdge(true, false);

            await _subProcedureService.MoveTo(input.EstimatedRightH.X - _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(false, false, 0);
            input.EstimatedRightVBottom = await _subProcedureService.FineProbeForEdge(false, true);

            await _subProcedureService.MoveTo(input.EstimatedRightH.X - _configuration.Config.AutoLocateAndMeasureStockOffsetOnVerticalProbing, 0, _configuration.Config.ZSafeHeight, _configuration.Config.ProbeFastSpeedF);
            await _subProcedureService.RoughlyProbeForEdge(false, true, 0);
            input.EstimatedRightVTop = await _subProcedureService.FineProbeForEdge(false, false);

            // Offset points by probe ball size
            input.EstimatedLeftVTop = _mathService.OffsetPoint(input.EstimatedLeftVTop, 0, -_configuration.CalibrationConfig.ProbeConfig.YBackwardsProbeOffset, 0);
            input.EstimatedRightVTop = _mathService.OffsetPoint(input.EstimatedRightVTop, 0, -_configuration.CalibrationConfig.ProbeConfig.YBackwardsProbeOffset, 0);
            input.EstimatedLeftVBottom = _mathService.OffsetPoint(input.EstimatedLeftVBottom, 0, _configuration.CalibrationConfig.ProbeConfig.YForwardProbeOffset, 0);
            input.EstimatedRightVBottom = _mathService.OffsetPoint(input.EstimatedRightVBottom, 0, _configuration.CalibrationConfig.ProbeConfig.YForwardProbeOffset, 0);
            input.EstimatedLeftH = _mathService.OffsetPoint(input.EstimatedLeftH, 0, _configuration.CalibrationConfig.ProbeConfig.XRightProbeOffset, 0);
            input.EstimatedRightH = _mathService.OffsetPoint(input.EstimatedRightH, 0, -_configuration.CalibrationConfig.ProbeConfig.XLeftProbeOffset, 0);
            return input;
        }

        private void PrintBottomOffset(AutoLocateAutoCalibrationResultModel input)
        {
            _loggerService.LogInfo("Result", "BottomOffset EstimatedLeftBottomCorner2H: " + input.EstimatedLeftBottomCorner2H);
            _loggerService.LogInfo("Result", "BottomOffset EstimatedLeftBottomCornerH: " + input.EstimatedLeftBottomCornerH);
            _loggerService.LogInfo("Result", "BottomOffset EstimatedLeftH: " + input.EstimatedLeftH);
            _loggerService.LogInfo("Result", "BottomOffset EstimatedLeftVBottom: " + input.EstimatedLeftVBottom);
            _loggerService.LogInfo("Result", "BottomOffset EstimatedLeftVTop: " + input.EstimatedLeftVTop);
            _loggerService.LogInfo("Result", "BottomOffset EstimatedRightBottomCorner2H: " + input.EstimatedRightBottomCorner2H);
            _loggerService.LogInfo("Result", "BottomOffset EstimatedRightBottomCornerH: " + input.EstimatedRightBottomCornerH);
            _loggerService.LogInfo("Result", "BottomOffset EstimatedRightH: " + input.EstimatedRightH);
            _loggerService.LogInfo("Result", "BottomOffset EstimatedRightVBottom: " + input.EstimatedRightVBottom);
            _loggerService.LogInfo("Result", "BottomOffset EstimatedRightVTop: " + input.EstimatedRightVTop);

        }

        private async Task<AutoLocateAutoCalibrationResultModel> CalibrateBottomOffset(AutoLocateAutoCalibrationResultModel input, decimal stockHeight)
        {
            // Find calibrationPoints Of bottom
            var oldHitHeight = _configuration.Config.ZHitHeight;
            try
            {
                _configuration.Config.ZHitHeight = -stockHeight + -_configuration.Config.ZHitHeight;

                var bottomOffsetCalibrationModel = new AutoLocateAutoCalibrationResultModel();
                await RoughCalibrate(bottomOffsetCalibrationModel);
                bottomOffsetCalibrationModel = await FineCalibrate(bottomOffsetCalibrationModel);

                // Calibrate offset
                return new AutoLocateAutoCalibrationResultModel()
                {
                    EstimatedLeftBottomCorner2H = _mathService.SubTractPoint2D(bottomOffsetCalibrationModel.EstimatedRightH, input.EstimatedRightH),
                    EstimatedLeftBottomCornerH = _mathService.SubTractPoint2D(bottomOffsetCalibrationModel.EstimatedLeftBottomCornerH, input.EstimatedLeftBottomCornerH),
                    EstimatedLeftH = _mathService.SubTractPoint2D(bottomOffsetCalibrationModel.EstimatedLeftH, input.EstimatedLeftH),
                    EstimatedLeftVBottom = _mathService.SubTractPoint2D(bottomOffsetCalibrationModel.EstimatedLeftVBottom, input.EstimatedLeftVBottom),
                    EstimatedLeftVTop = _mathService.SubTractPoint2D(bottomOffsetCalibrationModel.EstimatedLeftVTop, input.EstimatedLeftVTop),
                    EstimatedRightBottomCorner2H = _mathService.SubTractPoint2D(bottomOffsetCalibrationModel.EstimatedRightBottomCorner2H, input.EstimatedLeftH),
                    EstimatedRightBottomCornerH = _mathService.SubTractPoint2D(bottomOffsetCalibrationModel.EstimatedRightBottomCornerH, input.EstimatedRightBottomCornerH),
                    EstimatedRightH = _mathService.SubTractPoint2D(bottomOffsetCalibrationModel.EstimatedRightH, input.EstimatedRightH),
                    EstimatedRightVBottom = _mathService.SubTractPoint2D(bottomOffsetCalibrationModel.EstimatedRightVBottom, input.EstimatedRightVBottom),
                    EstimatedRightVTop = _mathService.SubTractPoint2D(bottomOffsetCalibrationModel.EstimatedRightVTop, input.EstimatedRightVTop)
                };
            }
            finally
            {
                _configuration.Config.ZHitHeight = oldHitHeight;
            }
        }

        public string GetName()
        {
            return "AutoLocateAndMeasureStock";
        }
    }
}
