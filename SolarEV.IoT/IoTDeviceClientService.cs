using FormsIoT.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SolarEV.IoT
{
    public class IoTDeviceClientService : IIoTDeviceClientService
    {
        private DeviceClient _deviceClient;
        private IDeviceConfigService _deviceConfigService;

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
                    Debug.WriteLine("Connected to IoT Hub", $"Registered Device Id: {_deviceConfigService.DeviceId}");
                else
                    Debug.WriteLine("Connection status changed", $"IoT Hub connection status: {_lastKnownConnectionStatus}");
            }
        }

        public ConnectionStatusChangeReason LastKnownConnectionChangeReason { get; set; }

        public event EventHandler<ConnectionProgressStatus> ConnectionStatusChanged;

        public IoTDeviceClientService(IDeviceConfigService deviceConfigService)
        {
            _deviceConfigService = deviceConfigService;
            LastKnownConnectionStatus = ConnectionProgressStatus.Disconnected;
        }

        private void ConnectionStatusChangesHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            LastKnownConnectionChangeReason = reason;
            LastKnownConnectionStatus = (ConnectionProgressStatus)status;
        }

        public async Task<bool> ConnectAsync()
        {
            var deviceId = _deviceConfigService.DeviceId;
            var deviceKey = _deviceConfigService.DeviceKey;

            if (string.IsNullOrEmpty(_deviceConfigService.AssignedEndPoint))
            {
                Debug.WriteLine($"Provisioning device: {deviceId}");
                await ProvisionAsync();
            }

            var options = new ClientOptions
            {
                ModelId = _deviceConfigService.ModelId
            };

            var authMethod = new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey);

            var csBuilder = IotHubConnectionStringBuilder.Create(_deviceConfigService.AssignedEndPoint, authMethod);
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

        public async Task SendReportedPropertiesAsync(ReportedDeviceProperties reportedDeviceProperties)
        {
            if (LastKnownConnectionStatus != ConnectionProgressStatus.Connected)
                return;

            var reportedPropsJson = JsonConvert.SerializeObject(reportedDeviceProperties);

            var twinc = new TwinCollection(reportedPropsJson);

            await _deviceClient.UpdateReportedPropertiesAsync(twinc).ConfigureAwait(false);

            var deviceInfo = reportedDeviceProperties.DeviceInfo;
            var formattedValue = $"Manufacturer: {deviceInfo.Manufacturer}\nModel: {deviceInfo.Model}\nOsName: {deviceInfo.OsName}\nProcessorArchitecture: {deviceInfo.ProcessorArchitecture}\nProcessorManufacturer: {deviceInfo.ProcessorManufacturer}\nSwVersion: {deviceInfo.SwVersion}\nTotalMemory: {deviceInfo.TotalMemory}\nTotalStorage: {deviceInfo.TotalStorage}";
            Debug.WriteLine($"Device properties sent to hub", formattedValue);
        }

        public async Task SendEventAsync(Sensors sensors)
        {
            if (LastKnownConnectionStatus != ConnectionProgressStatus.Connected)
                return;

            var sensorsData = JsonConvert.SerializeObject(sensors);

            var message = new Message(Encoding.UTF8.GetBytes(sensorsData));
            message.ComponentName = "sensors";
            message.ContentType = "application/json";
            message.ContentEncoding = "utf-8";

            await _deviceClient.SendEventAsync(message).ConfigureAwait(false);

            var formattedValue = $"Accelerometer: {sensors.Accelerometer?.X:0.00},{sensors.Accelerometer?.Y:0.00},{sensors.Accelerometer?.Z:0.00}\nBarometer: {sensors.Barometer}\nBattery: {sensors.Battery}\nGeolocation: {sensors.Geolocation?.Lat:0.00},{sensors.Geolocation?.Lon:0.00} ({sensors.Geolocation?.Alt:0.00})\nMagnetometer: {sensors.Magnetometer}\nOrientation: {sensors.Gyroscope.X:0.00},{sensors.Gyroscope.Y:0.00},{sensors.Gyroscope.Z:0.00},{sensors.Gyroscope.W:0.00}";
            Debug.WriteLine($"Sensors data sent to hub", formattedValue);
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
            var dpsGlobalEndpoint = _deviceConfigService.DpsGlobalEndpoint;
            var dpsIdScope = _deviceConfigService.ScopeId;
            var deviceId = _deviceConfigService.DeviceId;
            var modelId = _deviceConfigService.ModelId;
            var dpsSymetricKey = _deviceConfigService.DeviceKey;

            Debug.WriteLine("Provisioing device...");

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
                        _deviceConfigService.AssignedEndPoint = regResult.AssignedHub;
                        LastKnownConnectionStatus = ConnectionProgressStatus.Provisioned;
                    }
                    Debug.WriteLine($"Provisioned device: {deviceId}");

                    return true;
                }
            }
        }
    }
}
