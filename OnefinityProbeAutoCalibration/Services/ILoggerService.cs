namespace OnefinityProbeAutoCalibration.Services
{
    public interface ILoggerService
    {
        void LogInfo(string logSourceName, string message);
        void ReadLine();
    }
}