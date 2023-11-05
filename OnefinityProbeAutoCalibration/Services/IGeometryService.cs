using OnefinityProbeAutoCalibration.Models;
using System.Drawing;

namespace OnefinityProbeAutoCalibration.Services
{
    public interface IGeometryService
    {
        Point2D FindCenterOfCircleGivenThreePoints(Point2D a, Point2D b, Point2D c);
        Point2D RotatePoint(Point2D pointToRotate, Point2D centerPoint, double angleInDegrees);
    }
}