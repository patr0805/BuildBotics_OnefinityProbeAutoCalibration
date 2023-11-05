using OnefinityProbeAutoCalibration.Models;

namespace OnefinityProbeAutoCalibration.Services
{
    public interface IMathService
    {
        decimal ConvertRelativeToAbsolute(decimal pos, decimal offset);
        decimal ConvertAbsolutePositionToRelative(decimal pos, decimal offset);
        Point2D ConvertAbsolutePositionToRelative(Point2D pos, MachineState state);
        double ConvertRadiansToDegrees(double radians);
        double ConvertDegreesToRadians(double degrees);
        bool RelativeCoordinateEquals(MachineState state, decimal? x, decimal? y, decimal? z);
        decimal CalculatePoint2DDistance(Point2D pointA, Point2D pointB);
        Line CalculateLineFromPoints(Point2D pointA, Point2D pointB);
        Point2D CalculateLineIntersection(Line l1, Line l2);
        double AngleBetweenLines(Line line1, Line line2);
        Point2D OffsetPoint(Point2D point, decimal offsetX, decimal offsetY, int offsetZ);
        Point2D GetUnitVector(Point2D pointA, Point2D pointB);
        Line CalculateOrthogonalLine(Line a, Point2D b);
        Line OffsetLine(Line l, decimal offset);
        Point2D SubTractPoint2D(Point2D a, Point2D b);
        double AngleBetweenPoints(Point2D leftHoleCenterWithoutZ, Point2D rightHoleCenterWithoutZ);
    }
}