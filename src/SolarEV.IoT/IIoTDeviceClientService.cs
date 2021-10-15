using SolarEV.IoT.Models;
using Microsoft.Azure.Devices.Client;
using System;
using System.Threading.Tasks;

namespace SolarEV.IoT
{
    public interface IIoTDeviceClientService
    {
        event EventHandler<ConnectionProgressStatus> ConnectionStatusChanged;
        ConnectionProgressStatus LastKnownConnectionStatus { get; }

        Task<bool> ConnectAsync();

        Task<bool> DisconnectAsync();

        Task SendEventAsync(Telemetries solarData);
        //Task SendReportedPropertiesAsync(ReportedDeviceProperties reportedDeviceProperties);

        //event EventHandler<SensorCommandPayload> CommandReceivedHandler;

    }
}
