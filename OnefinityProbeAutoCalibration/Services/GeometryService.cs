using OnefinityProbeAutoCalibration.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace OnefinityProbeAutoCalibration.Services
{
    public class GeometryService : IGeometryService
    {
        public Point2D FindCenterOfCircleGivenThreePoints(Point2D a, Point2D b, Point2D c)
        {
            var D21x = Convert.ToDouble(b.X - a.X);
            var D21y = Convert.ToDouble(b.Y - a.Y);
            var D31x = Convert.ToDouble(c.X - a.X);
            var D31y = Convert.ToDouble(c.Y - a.Y);

            var F2 = 0.5d * (Math.Pow(D21x, 2) + Math.Pow(D21y, 2));
            var F3 = 0.5d * (Math.Pow(D31x, 2) + Math.Pow(D31y, 2));

            var M23xy = D21x * D31y - D21y * D31x;

            var F23x = F2 * D31x - F3 * D21x;
            var F23y = F2 * D31y - F3 * D21y;

            var Cx = Convert.ToDouble(a.X) + (M23xy * F23y) / Math.Pow(M23xy, 2);
            var Cy = Convert.ToDouble(a.Y) + (-M23xy * F23x) / Math.Pow(M23xy, 2);
            return new Point2D(Cx, Cy, Convert.ToDouble(a.Z));
        }
        /// <summary>
        /// Rotates one point around another
        /// </summary>
        /// <param name="pointToRotate">The point to rotate.</param>
        /// <param name="centerPoint">The center point of rotation.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <returns>Rotated point</returns>
        public Point2D RotatePoint(Point2D pointToRotate, Point2D centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            decimal cosTheta = Convert.ToDecimal(Math.Cos(angleInRadians));
            decimal sinTheta = Convert.ToDecimal(Math.Sin(angleInRadians));
            return new Point2D(
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y), 0m);           
        }
    }
}
