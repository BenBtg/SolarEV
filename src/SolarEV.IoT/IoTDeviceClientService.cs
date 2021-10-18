using SolarEV.IoT.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SolarEV.IoT
{
    public class IoTDeviceClientService : IIoTDeviceClientService
    {
        private DeviceClient _deviceClient;
        private readonly IOptions<DeviceConfig> _deviceConfig;
        private readonly ILogger<IIoTDeviceClientService> _log;

        #region IIoTDeviceClientService

        private ConnectionProgressStatus _lastKnownConnectionStatus = ConnectionProgressStatus.Disconnected;
        public ConnectionProgressStatus LastKnownConnectionStatus
        {
            get { return _lastKnownConnectionStatus; }
            set
            {
                if (_lastKnownConnectionStatus == value)
                    return;

                _lastKnownConnectionStatus = value;
                ConnectionStatusChanged?.Invoke(this, LastKnownConnectionStatus);

                if (_lastKnownConnectionStatus == ConnectionProgressStatus.Connected)
                    Debug.WriteLine("Connected to IoT Hub", $"Registered Device Id: {_deviceConfig.Value.DeviceID}");
                else
                    Debug.WriteLine("Connection status changed", $"IoT Hub connection status: {_lastKnownConnectionStatus}");
            }
        }

        public ConnectionStatusChangeReason LastKnownConnectionChangeReason { get; set; }

        public event EventHandler<ConnectionProgressStatus> ConnectionStatusChanged;

        public IoTDeviceClientService(IOptions<DeviceConfig> deviceConfig, ILogger<IIoTDeviceClientService> log)
        {
            _deviceConfig = deviceConfig;
            _log = log;
            LastKnownConnectionStatus = ConnectionProgressStatus.Disconnected;
        }

        private void ConnectionStatusChangesHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            LastKnownConnectionChangeReason = reason;
            LastKnownConnectionStatus = (ConnectionProgressStatus)status;
        }

        public async Task<bool> ConnectAsync()
        {
            var deviceId = _deviceConfig.Value.DeviceID;
            var deviceKey = _deviceConfig.Value.PrimaryKey;

            if (string.IsNullOrEmpty(_deviceConfig.Value.AssignedEndPoint))
            {
                Debug.WriteLine($"Provisioning device: {deviceId}");
                await ProvisionAsync();
            }

            var options = new ClientOptions
            {
                ModelId = _deviceConfig.Value.ModelId
            };

            var authMethod = new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey);

            var csBuilder = IotHubConnectionStringBuilder.Create(_deviceConfig.Value.AssignedEndPoint, authMethod);
            string connectionString = csBuilder.ToString();

            _deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt, options);

            _deviceClient.SetConnectionStatusChangesHandler(ConnectionStatusChangesHandler);

            LastKnownConnectionStatus = ConnectionProgressStatus.Connecting;

            await _deviceClient.OpenAsync().ConfigureAwait(false);

            await _deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChanged, _deviceClient).ConfigureAwait(false);

            return true;
        }

        public async Task<bool> DisconnectAsync()
        {
            if (_deviceClient != null)
            {
                Debug.WriteLine("Disconnecting from IoT Hub");

                await _deviceClient.CloseAsync();
                _deviceClient.Dispose();
                _deviceClient = null;
            }

            return true;
        }

        /*public async Task SendReportedPropertiesAsync(ReportedDeviceProperties reportedDeviceProperties)
        {
            if (LastKnownConnectionStatus != ConnectionProgressStatus.Connected)
                return;

            var reportedPropsJson = JsonConvert.SerializeObject(reportedDeviceProperties);

            var twinc = new TwinCollection(reportedPropsJson);

            await _deviceClient.UpdateReportedPropertiesAsync(twinc).ConfigureAwait(false);

            var deviceInfo = reportedDeviceProperties.DeviceInfo;
            var formattedValue = $"Manufacturer: {deviceInfo.Manufacturer}\nModel: {deviceInfo.Model}\nOsName: {deviceInfo.OsName}\nProcessorArchitecture: {deviceInfo.ProcessorArchitecture}\nProcessorManufacturer: {deviceInfo.ProcessorManufacturer}\nSwVersion: {deviceInfo.SwVersion}\nTotalMemory: {deviceInfo.TotalMemory}\nTotalStorage: {deviceInfo.TotalStorage}";
            Debug.WriteLine($"Device properties sent to hub", formattedValue);
        }*/

        public async Task SendEventAsync(Telemetries solarData)
        {
            if (LastKnownConnectionStatus != ConnectionProgressStatus.Connected)
                return;

            var sensorsData = JsonConvert.SerializeObject(solarData);

            var message = new Message(Encoding.UTF8.GetBytes(sensorsData));
            message.ContentType = "application/json";
            message.ContentEncoding = "utf-8";

            await _deviceClient.SendEventAsync(message).ConfigureAwait(false);

            //var formattedValue = $"Exporting: {solarData?.Exporting.X:0.00}";
            //Debug.WriteLine($"Sensors data sent to hub", formattedValue);
        }

        #endregion
        static ProvisioningRegistrationAdditionalData GetProvisionPayload(string modelId)
        {
            return new ProvisioningRegistrationAdditionalData
            {
                JsonData = "{ modelId: '" + modelId + "'}"
            };
        }

        private static async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("One or more device twin desired properties changed:");
            Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

            var client = userContext as DeviceClient;

            var reportedProperties = new TwinCollection
            {
                ["DateTimeLastDesiredPropertyChangeReceived"] = DateTime.Now
            };

            await client.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);

            Console.WriteLine("Sent current time as reported property to device twin");
        }

        private async Task<bool> ProvisionAsync()
        {
            var dpsGlobalEndpoint = _deviceConfig.Value.DpsGlobalEndpoint;
            var dpsIdScope = _deviceConfig.Value.IDScope;
            var deviceId = _deviceConfig.Value.DeviceID;
            var modelId = _deviceConfig.Value.ModelId;
            var dpsSymetricKey = _deviceConfig.Value.PrimaryKey;

            Debug.WriteLine("Provisioning device...");

            LastKnownConnectionStatus = ConnectionProgressStatus.Provisioning;

            using (var security = new SecurityProviderSymmetricKey(deviceId, dpsSymetricKey, null))
            {
                Debug.WriteLine($"Security: {deviceId},{dpsSymetricKey}");
                using (var transport = new ProvisioningTransportHandlerMqtt())
                {
                    var provisioningClient = ProvisioningDeviceClient.Create(dpsGlobalEndpoint, dpsIdScope, security, transport);

                    var payload = GetProvisionPayload(modelId);
                    Debug.WriteLine("Registering");
                    var regResult = await provisioningClient.RegisterAsync(payload);

                    if (regResult.Status == ProvisioningRegistrationStatusType.Assigned)
                    {
                        _deviceConfig.Value.AssignedEndPoint = regResult.AssignedHub;
                        LastKnownConnectionStatus = ConnectionProgressStatus.Provisioned;
                    }
                    Debug.WriteLine($"Provisioned device: {deviceId}");

                    return true;
                }
            }
        }
    }
}
