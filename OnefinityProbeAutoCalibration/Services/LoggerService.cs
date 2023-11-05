using System;
namespace OnefinityProbeAutoCalibration.Services
{
    public class LoggerService : ILoggerService
    {
        public void LogInfo(string logSourceName, string message)
        {
            Console.WriteLine(logSourceName + " : " + message);
        }

        public void ReadLine()
        {
            Console.ReadLine();
        }
    }
}
