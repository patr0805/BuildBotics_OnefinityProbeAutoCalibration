using System;
using System.Collections.Generic;
using System.Text;

namespace OnefinityProbeAutoCalibration.Configuration
{
    public class CalibrationConfiguration
    {
        public ProbeCalibrationConfiguration ProbeConfig { get; set; }
        public DoTopCalibrationUsingHolesConfig DoTopCalibrationUsingHoles { get; set; }
    }
}
