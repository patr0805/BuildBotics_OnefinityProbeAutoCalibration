namespace OnefinityProbeAutoCalibration.Models
{
    public class AutoLocateAutoCalibrationResultModel
    {
        public Point2D EstimatedLeftH { get; set; }
        public Point2D EstimatedLeftVBottom { get; set; }
        public Point2D EstimatedLeftVTop { get; set; }
        public Point2D EstimatedRightH { get; set; }
        public Point2D EstimatedRightVBottom { get; set; }
        public Point2D EstimatedRightVTop { get; set; }
        public Point2D EstimatedLeftBottomCornerH { get; set; }
        public Point2D EstimatedLeftBottomCorner2H { get; set; }
        public Point2D EstimatedRightBottomCornerH { get; set; }
        public Point2D EstimatedRightBottomCorner2H { get; set; }
    }
}
