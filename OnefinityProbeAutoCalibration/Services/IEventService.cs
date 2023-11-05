using OnefinityProbeAutoCalibration.Models;
using System;

namespace OnefinityProbeAutoCalibration.Services
{
    public interface IEventService
    {
        void WaitUntil(string state);
        void MachineStateUpdated(string id);
        void SubscribeMoveTo(string id, decimal? x, decimal? y, decimal? z, Action completeAction);
        void OnefinityDoneReceived(string id);
        bool CheckForEventDestinationReached();
        void SubscribeCommandCompletion(string id, Action completeAction);
        void MDIModeFinished();
        void WaitForMDIModeFinished();
        void AcceptMDIModeFinished();
    }
}