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

namespace SolarEV.TransportProtocols.Utilities
{
    public class SolarEV
    {
        public static async Task Main(string[] args)
        {
            // instantiate startup
            // all the constructor logic would happen
            var startup = new Startup();

            // request an instance of type ISomeService
            // from the ServicePipeline built
            // returns an object of type SomeService
            var solarListener = startup.Provider.GetRequiredService<ISolarListener>();
            var deviceClientService = startup.Provider.GetRequiredService<IIoTDeviceClientService>();

            // call DoProcess on the ISomeService type
            // should print value for SomeKey on console
            // fetched from IConfiguration
            // injected into the class via DI

            solarListener.SolarMessageReceived += SolarListener_SolarMessageReceived;

            await deviceClientService.ConnectAsync();
            await solarListener.StartListeningAsync();

            Console.WriteLine("End");
        }

        private static void SolarListener_SolarMessageReceived(object sender, SolarMessageEventArgs e)
        {
            Console.WriteLine(e.Data.Day.Generated);
        }
    }
}