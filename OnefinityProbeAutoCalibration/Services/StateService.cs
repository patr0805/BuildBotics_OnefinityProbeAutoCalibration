using OnefinityProbeAutoCalibration.Models;
using System.Collections.Generic;
using System.Threading;

namespace OnefinityProbeAutoCalibration.Services
{
    public class StateService : IStateService
    {
        private static HashSet<string> _ackedMessages = new HashSet<string>();
        private static MachineState _machineState = new MachineState();
        private static object _sendLock = new object();
        private static int _id = 0;

        public HashSet<string> GetAckedMessages()
        {
            return _ackedMessages;
        }

        public MachineState GetMachineState()
        {
            return _machineState;
        }

        public object GetSendingLock()
        {
            return _sendLock;
        }

        public int GetNextId()
        {
            return Interlocked.Increment(ref _id);
        }
    }
}
