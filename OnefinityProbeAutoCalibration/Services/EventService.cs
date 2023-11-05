using OnefinityProbeAutoCalibration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static OnefinityProbeAutoCalibration.Models.Event;

namespace OnefinityProbeAutoCalibration.Services
{
    public class EventService : IEventService
    {
        private static Object lockObject = new object();
        private static List<Event> events = new List<Event>();
        private IStateService _stateService;
        private IMathService _mathService;
        private static SemaphoreSlim mdiFinishedEvent = new SemaphoreSlim(0, 1);
        private static bool acceptMdiEventRelease = false;
        public EventService(IStateService stateService,
            IMathService mathService)
        {
            _stateService = stateService;
            _mathService = mathService;
        }

        public bool CheckForEventDestinationReached()
        {
            lock (lockObject)
            {
                var xCordinate = _mathService.ConvertAbsolutePositionToRelative(_stateService.GetMachineState().xp, _stateService.GetMachineState().offset_x);
                var yCordinate = _mathService.ConvertAbsolutePositionToRelative(_stateService.GetMachineState().yp, _stateService.GetMachineState().offset_y);
                var zCordinate = _mathService.ConvertAbsolutePositionToRelative(_stateService.GetMachineState().zp, _stateService.GetMachineState().offset_z);
                foreach (var e in events)
                {
                    if (e.expectedXMoveTo != null && e.expectedXMoveTo != xCordinate)
                    {
                        continue;
                    }

                    if (e.expectedYMoveTo != null && e.expectedYMoveTo != yCordinate)
                    {
                        continue;
                    }

                    if (e.expectedZMoveTo != null && e.expectedZMoveTo == zCordinate)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void MachineStateUpdated(string id)
        {
            lock (lockObject)
            {
                foreach (var e in events.ToList())
                {
                    if (e.id == id || id == null)
                    {
                        e.ExectureComplete();
                        events.Remove(e);
                    }
                }
            }
        }

        public void WaitUntil(string state)
        {
            while (true)
            {
                while (_stateService.GetMachineState().xx != state)
                {
                    System.Threading.Thread.Sleep(100);
                }

                for (int retainedFor = 0; retainedFor <= 1000; retainedFor += 100)
                {
                    if (retainedFor >= 1000)
                    {
                        return;
                    }

                    if (_stateService.GetMachineState().xx != state)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        public void SubscribeMoveTo(string id, decimal? x, decimal? y, decimal? z, Action completeAction)
        {
            lock (lockObject)
            {
                var e = new Event()
                {
                    id = id,
                    expectedXMoveTo = x,
                    expectedYMoveTo = y,
                    expectedZMoveTo = z
                };

                e.MoveToCompleted += new MoveToComplete(completeAction);
                events.Add(e);
            }
        }

        public void OnefinityDoneReceived(string id)
        {
            MachineStateUpdated(id);
        }

        public void SubscribeCommandCompletion(string id, Action completeAction)
        {
            lock (lockObject)
            {
                var e = new Event()
                {
                    id = id
                };

                e.MoveToCompleted += new MoveToComplete(completeAction);
                events.Add(e);
            }
        }

        public void MDIModeFinished()
        {
            lock (mdiFinishedEvent)
            {
                if (!acceptMdiEventRelease)
                {
                    return;
                }

                acceptMdiEventRelease = false;
                mdiFinishedEvent.Release();
            }
        }

        public void WaitForMDIModeFinished()
        {
            mdiFinishedEvent.Wait();
        }

        public void AcceptMDIModeFinished()
        {
            lock (mdiFinishedEvent)
            {
                if (acceptMdiEventRelease)
                {
                    throw new Exception("Shouldnt ever hit this!");
                }

                acceptMdiEventRelease = true;
            }
        }
    }
}
