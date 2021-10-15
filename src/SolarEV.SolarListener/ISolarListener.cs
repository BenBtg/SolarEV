using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SolarEV.Models;

namespace SolarEV.Services
{
    public interface ISolarListener
    {
        event EventHandler<SolarMessageEventArgs> SolarMessageReceived;

        Task StartListeningAsync();
        Task StopListeningAsync();
    }

    public class SolarMessageEventArgs : EventArgs
    {
        public Solar Data;
        public SolarMessageEventArgs(Solar data)
        {
            Data = data;
        }
    }
}