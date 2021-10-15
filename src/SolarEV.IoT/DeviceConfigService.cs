using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SolarEV.IoT
{
    public class DeviceConfigService : IDeviceConfigService
    {
        private string deviceId;
        private string scopeId;
        private string deviceKey;

        public string DeviceId { get => deviceId; set => deviceId = value; }
        public string ScopeId { get => scopeId; set => scopeId = value; }
        public string ModelId { get => "dtmi:azureiot:SolarDevice;2"; }
        public string DeviceKey { get => deviceKey; set => deviceKey = value; }


        // public Task InitAsync()
        // {
        //     // DeviceId = await LoadValue(nameof(DeviceId));
        //     // ScopeId = await LoadValue(nameof(ScopeId));
        //     // DeviceKey = await LoadValue(nameof(DeviceKey));
        // }

        public void ResetConfig()
        {
            //Xamarin.Essentials.SecureStorage.RemoveAll();
        }

        // public Task SaveAsync()
        // {
        //     // await SaveValue(DeviceId, nameof(DeviceId));
        //     // await SaveValue(ScopeId, nameof(ScopeId));
        //     // await SaveValue(DeviceKey, nameof(DeviceKey));
        // }

        // private string LoadValue(string propertyName)
        // {
        //     return String.Empty; //await Xamarin.Essentials.SecureStorage.GetAsync(propertyName);
        // }

        // private SaveValue(string value, string propertyName)
        // {
        //     //await Xamarin.Essentials.SecureStorage.SetAsync(propertyName, value);
        // }

        public string DpsGlobalEndpoint => "global.azure-devices-provisioning.net";

        public string AssignedEndPoint { get; set; }

    }
}
