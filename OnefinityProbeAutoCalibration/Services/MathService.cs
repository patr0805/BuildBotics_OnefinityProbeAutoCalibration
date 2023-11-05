using OnefinityProbeAutoCalibration.Models;
using System;
using System.Reflection.Metadata;

namespace OnefinityProbeAutoCalibration.Services
{
    public class MathService : IMathService
    {
        public decimal ConvertRelativeToAbsolute(decimal pos, decimal offset)
        {
            return pos + offset;
        }

        public decimal ConvertAbsolutePositionToRelative(decimal pos, decimal offset)
        {
            return pos - offset;
        }

        public Point2D ConvertAbsolutePositionToRelative(Point2D pos, MachineState state)
        {
            return new Point2D(ConvertAbsolutePositionToRelative(pos.X, state.offset_x), ConvertAbsolutePositionToRelative(pos.Y, state.offset_y), ConvertAbsolutePositionToRelative(pos.Z, state.offset_z));
        }

        public double ConvertRadiansToDegrees(double radians)
        {
            double degrees = (180 / Math.PI) * radians;
            return (degrees);
        }

        public double ConvertDegreesToRadians(double degrees)
        {
            return Math.PI * degrees / 180.0;
        }

        public bool RelativeCoordinateEquals(MachineState state, decimal? x, decimal? y, decimal? z)
        {
            var tolerance = 0.05m;
            return (x == null || Math.Abs(ConvertAbsolutePositionToRelative(state.xp, state.offset_x) - x.Value) < tolerance) &&
                (y == null || Math.Abs(ConvertAbsolutePositionToRelative(state.yp, state.offset_y) - y.Value) < tolerance) &&
                (z == null || Math.Abs(ConvertAbsolutePositionToRelative(state.zp, state.offset_z) - z.Value) < tolerance);
        }

        public decimal CalculatePoint2DDistance(Point2D a, Point2D b)
        {
            double xDiff = Convert.ToDouble(b.X - a.X);
            double yDiff = Convert.ToDouble(b.Y - a.Y);
            return Convert.ToDecimal(Math.Sqrt(xDiff * xDiff + yDiff * yDiff));
        }

        public Line CalculateLineFromPoints(Point2D a, Point2D b)
        {
            decimal slope = (b.Y - a.Y) / (b.X - a.X);
            decimal intercept = a.Y - (slope * a.X);
            return new Line(slope, intercept);
        }

        public Point2D CalculateLineIntersection(Line l1, Line l2)
        {
            decimal x = (l2.B - l1.B) / (l1.A - l2.A);
            decimal y = l1.A * x + l1.B;
            return new Point2D(x, y, 0);
        }

        public double AngleBetweenLines(Line line1, Line line2)
        {
            // Calculate the angles of the lines in radians
            double angle1 = Math.Atan(Convert.ToDouble(line1.A));
            double angle2 = Math.Atan(Convert.ToDouble(line2.A));

            // Return the absolute difference between the angles in degrees
            return ConvertRadiansToDegrees(Math.Abs(angle1 - angle2) * 180 / Math.PI);
        }

        public Point2D OffsetPoint(Point2D point, decimal offsetX, decimal offsetY, int offsetZ)
        {
            return new Point2D(point.X + offsetX, point.Y + offsetY, point.Z + offsetZ);
        }

        public Point2D GetUnitVector(Point2D pointA, Point2D pointB)
        {
            var unitVectorX = (pointB.X - pointA.X) / CalculatePoint2DDistance(pointA, pointB);
            var unitVectorY = (pointB.Y - pointA.Y) / CalculatePoint2DDistance(pointA, pointB);
            return new Point2D(unitVectorX, unitVectorY, pointA.Z);
        }

        public Line CalculateOrthogonalLine(Line a, Point2D p)
        {
            decimal orthogonalSlope = -1m / a.A;
            decimal orthogonalIntercept = p.Y - orthogonalSlope * p.X;

            return new Line(orthogonalSlope, orthogonalIntercept);
        }

        public Line OffsetLine(Line l, decimal offset)
        {
            return new Line(l.A, l.B + offset);
        }

        public Point2D SubTractPoint2D(Point2D a, Point2D b)
        {
            return new Point2D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public double AngleBetweenPoints(Point2D point1, Point2D point2)
        {
            double deltaX = Convert.ToDouble(point2.X - point1.X);
            double deltaY = Convert.ToDouble(point2.Y - point1.Y);
            return Math.Atan2(deltaY, deltaX) * 180.0 / Math.PI;
        }
    }
}
