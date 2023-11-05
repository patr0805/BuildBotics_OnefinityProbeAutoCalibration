using OnefinityProbeAutoCalibration.Models;

namespace OnefinityProbeAutoCalibration.Configuration
{
    public class StockConfiguration
    {
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public AutoLocateAutoCalibrationResultModel BottomOffset { get; set; }
    }
}
