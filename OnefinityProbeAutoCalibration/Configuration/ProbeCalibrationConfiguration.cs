using System;
using System.Collections.Generic;
using System.Text;

namespace OnefinityProbeAutoCalibration.Configuration
{
    public class ProbeCalibrationConfiguration
    {
        public decimal XRightProbeOffset { get; set; }
        public decimal XLeftProbeOffset { get; set; }
        public decimal YForwardProbeOffset { get; set; }
        public decimal YBackwardsProbeOffset { get; set; }
        public decimal ZProbeOffset { get; set; }
    }
}
