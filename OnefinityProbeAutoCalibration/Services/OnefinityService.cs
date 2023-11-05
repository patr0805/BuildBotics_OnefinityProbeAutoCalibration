using Newtonsoft.Json;
using OnefinityProbeAutoCalibration.Configuration;
using OnefinityProbeAutoCalibration.Models;
using syp.biz.SockJS.NET.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OnefinityProbeAutoCalibration.Services
{
    public class OnefinityService : IOnefinityService
    {
        private const string LogSourceName = "OnefinityService";
        private AppConfiguration _configuration;
        private ILoggerService _loggerService;
        private static SockJS sockJS;
        public delegate void DataReceivedHandler(string data);
        private static event DataReceivedHandler DataReceived;
        private IStateService _stateService;
        private IEventService _eventService;
        private IMathService _mathService;
        private static bool stopSent = false;
        private static object processing = new object();

        public OnefinityService(AppConfiguration configuration, ILoggerService loggerService, IStateService stateService, IEventService eventService, IMathService mathService)
        {
            _configuration = configuration;
            _loggerService = loggerService;
            _stateService = stateService;
            _eventService = eventService;
            _mathService = mathService;
        }

        public void Home()
        {
            lock (_stateService.GetSendingLock())
            {
                Thread.Sleep(500);
                using (var client = new System.Net.WebClient())
                {
                    client.UploadData($"http://{_configuration.Config.OnefinityIPAddress}/api/home", "PUT", new byte[0]);
                }

                while (_stateService.GetMachineState().xx == "READY")
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void Stop(bool skipLock = false)
        {
            if (skipLock)
            {
                Thread.Sleep(500);
                using (var client = new System.Net.WebClient())
                {
                    client.UploadData($"http://{_configuration.Config.OnefinityIPAddress}/api/stop", "PUT", new byte[0]);
                }

                while (_stateService.GetMachineState().xx != "READY")
                {
                    Thread.Sleep(100);
                }

                Thread.Sleep(1000);
                return;
            }

            lock (_stateService.GetSendingLock())
            {
                Thread.Sleep(500);
                using (var client = new System.Net.WebClient())
                {
                    client.UploadData($"http://{_configuration.Config.OnefinityIPAddress}/api/stop", "PUT", new byte[0]);
                }

                while (_stateService.GetMachineState().xx != "READY")
                {
                    Thread.Sleep(100);
                }

                Thread.Sleep(1000);
            }
        }

        public async Task Init()
        {
            _loggerService.LogInfo("Program", "Connecting CNC");
            var config = new syp.biz.SockJS.NET.Client.Configuration()
            {
                BaseEndpoint = new Uri($"http://{_configuration.Config.OnefinityIPAddress}/sockjs"),
                Cookies = new CookieContainer()
            };

            config.Cookies.Add(new Cookie()
            {
                Name = "client-id",
                Domain = _configuration.Config.OnefinityIPAddress,
                Value = "sBSigd5P4fJCoIApQdgMu3Xra69qOTHNYl5Yb6URLriCk8Htq1q_"
            }); ;

            sockJS = new SockJS(config);
            sockJS.Connected += (sender, e) =>
            {
                _loggerService.LogInfo(LogSourceName, "Connected");
            };

            sockJS.Message += async (sender, msg) =>
            {
                if (DataReceived != null)
                {
                    DataReceived(msg);
                }
            };

            sockJS.Disconnected += (sender, e) =>
            {
                // this event is triggered when the connection is disconnected (for any reason)
                _loggerService.LogInfo("Program", "CNC diconnected!");
                throw new Exception("CNC was disconnected! All ongoing tasks have been aborted.");
            };

            await sockJS.Connect(); // connect to the server
            _loggerService.LogInfo("Program", "Connected");

            Subscribe(".", async (data) =>
            {
                var cycleBefore = _stateService.GetMachineState().cycle;
                JsonConvert.PopulateObject(data, _stateService.GetMachineState());

                if (cycleBefore == "mdi" && _stateService.GetMachineState().cycle != "mdi")
                {
                    stopSent = false;
                    _eventService.MDIModeFinished();
                }

                if (!stopSent)
                {
                    lock (processing)
                    {
                        if (cycleBefore == "mdi" && _stateService.GetMachineState().xx == "HOLDING" && _stateService.GetMachineState().cycle == "mdi")
                        {
                            stopSent = true;
                            Stop(true);
                        }
                    }
                }
            });
        }

        public void ScheduleSend(int id, string message)
        {
            if (sockJS == null)
            {
                throw new Exception("No connection to CNC!");
            }

            lock (_stateService.GetSendingLock())
            {
                _loggerService.LogInfo("Debug", message);
                SendRaw(id.ToString(), message);
            }
        }

        public void SendRaw(string id, string message)
        {
            lock (_stateService.GetSendingLock())
            {
                while (_stateService.GetMachineState().xx != "READY" && _stateService.GetMachineState().cycle == "mdi")
                {
                    Thread.Sleep(100);
                }

                _eventService.AcceptMDIModeFinished();
                sockJS.Send(message);
                _eventService.WaitForMDIModeFinished();
                _eventService.OnefinityDoneReceived(id);
            }
        }

        private void HitActionOnRegexMatch(string regexString, string data, Action<string> action)
        {
            var regex = new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (regex.IsMatch(data))
            {
                action(data);
            }
        }

        public void Subscribe(string regex, Action<string> action)
        {
            DataReceived += new DataReceivedHandler((data) => HitActionOnRegexMatch(regex, data, action));
        }

        public void AckMessage(string id)
        {
            lock (_stateService.GetSendingLock())
            {
                while (_stateService.GetMachineState().xx != "READY")
                {
                    Thread.Sleep(100);
                }

                using (var client = new WebClient())
                {
                    client.UploadData($"http://{_configuration.Config.OnefinityIPAddress}/api/message/{id}/ack", "PUT", new byte[0]);
                }

                while (_stateService.GetMachineState().messages.Any(m => m.id == id))
                {
                    Thread.Sleep(100);
                }
            }
        }

        public async Task ZeroAxis(bool x, bool y, bool z)
        {
            while (_stateService.GetMachineState().xx != "READY")
            {
                Thread.Sleep(100);
            }

            var id = _stateService.GetNextId();
            var pass = new SemaphoreSlim(0, 1);
            _eventService.SubscribeCommandCompletion(id.ToString(), () => pass.Release());

            lock (_stateService.GetSendingLock())
            {
                using (var client = new WebClient())
                {
                    var xPart = x ? "X0 " : "";
                    var yPart = y ? "Y0 " : "";
                    var zPart = z ? "Z0" : "";
                    if (!string.IsNullOrEmpty(xPart + yPart + zPart))
                    {
                        ScheduleSend(id, "G92 " + xPart + yPart + zPart);
                    }
                }
            }

            await pass.WaitAsync();
            while ((x && (_stateService.GetMachineState().offset_x != _stateService.GetMachineState().xp)) ||
                (y && (_stateService.GetMachineState().offset_y != _stateService.GetMachineState().yp)) ||
                (z && (_stateService.GetMachineState().offset_z != _stateService.GetMachineState().zp)))
            {
                await Task.Delay(200);
            }
        }

        public Point2D GetCurrentRelativePosition()
        {
            return new Point2D(
                _mathService.ConvertAbsolutePositionToRelative(_stateService.GetMachineState().xp, _stateService.GetMachineState().offset_x),
                _mathService.ConvertAbsolutePositionToRelative(_stateService.GetMachineState().yp, _stateService.GetMachineState().offset_y),
                _mathService.ConvertAbsolutePositionToRelative(_stateService.GetMachineState().zp, _stateService.GetMachineState().offset_z)
                );
        }

        public Task SetZeroOffset(Point2D point)
        {
            var currentPosition = GetCurrentRelativePosition();
            var adjustedX = currentPosition.X - point.X;
            var adjustedY = currentPosition.Y - point.Y;
            var adjustedZ = currentPosition.Z - point.Z;
            var xString = $"X{string.Format("{0:0.###}", adjustedX).Replace(",", ".")}";
            var yString = $"Y{string.Format("{0:0.###}", adjustedY).Replace(",", ".")}";
            var zString = $"Z{string.Format("{0:0.###}", adjustedZ).Replace(",", ".")}";
            var combinedParts = (xString + " " + yString + " " + zString).Replace("  ", " ").Trim();
            var commandToSend = $"G92 {combinedParts}";
            var id = _stateService.GetNextId();
            var pass = new SemaphoreSlim(0, 1);
            _eventService.SubscribeCommandCompletion(id.ToString(), () => pass.Release());

            ScheduleSend(id, commandToSend);
            return pass.WaitAsync();
        }
    }
}
