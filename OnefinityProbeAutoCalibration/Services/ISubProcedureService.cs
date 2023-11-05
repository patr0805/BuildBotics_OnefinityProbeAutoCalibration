using OnefinityProbeAutoCalibration.Models;
using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration.Services
{
    public interface ISubProcedureService
    {
        Task MoveTo(decimal? x, decimal? y, decimal? z, int? f);
        Task<bool> ProbeTowards(decimal? x, decimal? y, decimal? z, int? f, bool throwExceptionOnNonHit = true);
        Task<Point2D> CalibrateCenter(bool skipZ = false);
        Task<Point2D> RoughlyProbeForEdge(bool horizontal, bool rightOrUp, decimal stockTopZ);
        Task<Point2D> FineProbeForEdge(bool horizontal, bool rightOrUp, bool skipZAdjustment = false);
        Task<Point2D> CalibrateCenterVH(bool skipZ = false);
    }
}
