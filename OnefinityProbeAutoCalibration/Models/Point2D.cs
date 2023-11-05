using System;

namespace OnefinityProbeAutoCalibration.Models
{
    public class Point2D
    {
        public Point2D(double x, double y, double z)
        {
            X = Convert.ToDecimal(x);
            Y = Convert.ToDecimal(y);
            Z = Convert.ToDecimal(z);
        }

        public Point2D(decimal x, decimal y, decimal z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal Z { get; set; }
    }
}
