using System;
using System.Collections.Generic;
using System.Text;

namespace OnefinityProbeAutoCalibration.Configuration
{
    public class GenerelConfig
    {
        public string OnefinityIPAddress { get; set; }
        public int ProbeMoveOffset { get; set; }
        public int ProbeSpeedF { get; set; }
        public int ZSafeHeight { get; set; }
        public decimal ZHitHeight { get; set; }
        public int ProbeFastSpeedF { get; set; }
        public int ProbeFastSuperSpeedF { get; set; }
        public int ProbeMoveOffsetSmall { get; set; }
        public int CalibrationCubeWidth { get; set; }
        public int CalibrationCubeSideWidth { get; set; }
        public int CalibrationHoleDiameter { get; set; }
        public int CalibrationExtraLimit { get; set; }
        public decimal ZCalibrationHitLimit { get; set; }
        public string Type { get; set; }
        public decimal MachineWidth { get; set; }
        public decimal MachineLength { get; set; }
        public decimal ZHitSmallHeight { get; set; }
        public decimal AutoLocateAndMeasureStockJumpDistance { get; set; }
        public decimal AutoLocateAndMeasureStockOffsetOnVerticalProbing { get; set; }
        public int ProbeSpeedRoughF { get; set; }
        public int HeightMapResolution { get; set; }
        public decimal CalibrationSideOffset { get; set; }
    }
}
