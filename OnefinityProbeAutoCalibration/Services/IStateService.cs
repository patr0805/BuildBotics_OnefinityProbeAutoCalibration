using OnefinityProbeAutoCalibration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnefinityProbeAutoCalibration.Services
{
    public interface IStateService
    {
        object GetSendingLock();
        HashSet<string> GetAckedMessages();
        MachineState GetMachineState();
        int GetNextId();
    }
}
