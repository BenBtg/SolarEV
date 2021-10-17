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


            await Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ConsoleHostedService>();
                })
                .RunConsoleAsync();

            Console.WriteLine("End");

        }


    }
}