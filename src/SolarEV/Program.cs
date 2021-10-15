using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SolarEV.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarEV.IoT;
using System.Threading;
using SolarEV.IoT.Models;
using SolarEV.Models;

namespace SolarEV.TransportProtocols.Utilities
{
    public class SolarEV
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);
        static IIoTDeviceClientService _deviceClientService;
        public static async Task Main(string[] args)
        {
            // instantiate startup
            // all the constructor logic would happen
            var startup = new Startup();

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


            _quitEvent.WaitOne();
            Console.WriteLine("End");

        }

        private static async void SolarListener_SolarMessageReceived(object sender, SolarMessageEventArgs e)
        {
            Console.WriteLine(e.Data.Day.Generated);
            var solarData = ConvertSolarToJson(e.Data);
            await _deviceClientService.SendEventAsync(solarData);
        }

        static Telemetries ConvertSolarToJson(Solar solar)
        {
            //var solarData = new SolarData();
            var solarData = new Telemetries
            {
                Id = solar.Id,
                Timestamp = solar.Timestamp,
                Exporting = solar.Current.Exporting.Text,
                Exported = solar.Day.Exported.Text,
                Generating = solar.Current.Generating.Text,
                Generated = solar.Day.Generated.Text,
            };
            return solarData;
        }
    }
}