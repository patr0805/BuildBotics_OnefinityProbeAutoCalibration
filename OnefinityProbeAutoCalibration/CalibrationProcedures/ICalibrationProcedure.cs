using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration.CalibrationProcedures
{
    public interface ICalibrationProcedure
    {
        Task Apply(decimal stockWidth, decimal stockLength, decimal stockHeight);
        string GetName();
    }
}
