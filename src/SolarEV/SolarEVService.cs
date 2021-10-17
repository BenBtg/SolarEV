using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarEV
{
    class SolarEVService
    {
        public void StartAsync()
        {
            // request an instance of type ISomeService
            // from the ServicePipeline built
            // returns an object of type SomeService
            var solarListener = startup.Provider.GetRequiredService<ISolarListener>();
            var deviceConfigService = startup.Provider.GetRequiredService<IDeviceConfigService>();
            var deviceID = startup.Configuration["DeviceID"];

            deviceConfigService.DeviceId = deviceID;
            deviceConfigService.DeviceKey = startup.Configuration["PrimaryKey"];
            deviceConfigService.ScopeId = startup.Configuration["IDScope"];

            _deviceClientService = startup.Provider.GetRequiredService<IIoTDeviceClientService>();


            // call DoProcess on the ISomeService type
            // should print value for SomeKey on console
            // fetched from IConfiguration
            // injected into the class via DI



            solarListener.SolarMessageReceived += SolarListener_SolarMessageReceived;

            await _deviceClientService.ConnectAsync();
            await solarListener.StartListeningAsync();

        }
    }
}
