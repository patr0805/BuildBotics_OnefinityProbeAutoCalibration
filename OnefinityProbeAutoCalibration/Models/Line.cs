using System;
using System.Collections.Generic;
using System.Text;

namespace OnefinityProbeAutoCalibration.Models
{
    public class Line
    {
        public Line(decimal a, decimal b)
        {
            A = a;
            B = b;
        }

        public decimal A { get; set; }
        public decimal B { get; set; }
    }
}
