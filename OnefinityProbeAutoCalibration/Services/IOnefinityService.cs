using OnefinityProbeAutoCalibration.Models;
using System;
using System.Threading.Tasks;
namespace OnefinityProbeAutoCalibration.Services
{
    public interface IOnefinityService
    {
        Task Init();
        void Home();
        void Stop(bool skipLock = false);
        void Subscribe(string data, Action<string> action);
        void ScheduleSend(int id, string message);
        void AckMessage(string id);
        Task ZeroAxis(bool x, bool y, bool z);
        Point2D GetCurrentRelativePosition();
        Task SetZeroOffset(Point2D point);
    }
}
