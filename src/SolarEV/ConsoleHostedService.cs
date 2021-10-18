using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SolarEV.IoT;
using SolarEV.IoT.Models;
using SolarEV.Models;
using SolarEV.Services;

namespace SolarEV
{
    internal sealed class ConsoleHostedService : IHostedService
    {
        private readonly ILogger _log;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ISolarListener _solarListener;
        private readonly IIoTDeviceClientService _deviceClient;

        private int? _exitCode;

        public ConsoleHostedService(
            ILogger<ConsoleHostedService> log,
            IHostApplicationLifetime appLifetime,
            ISolarListener solarListener,
            IIoTDeviceClientService deviceClientService)
        {
            _log = log;
            _appLifetime = appLifetime;
            _solarListener = solarListener;
            _deviceClient = deviceClientService;
            
            _solarListener.SolarMessageReceived += SolarListener_SolarMessageReceived;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogDebug($"Starting with arguments: {string.Join(" ", Environment.GetCommandLineArgs())}");

            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await _deviceClient.ConnectAsync();
                        await _solarListener.StartListeningAsync();
                        
                        _log.LogInformation($"Started");
                   
                        _exitCode = 0;
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "Unhandled exception!");
                        _exitCode = 1;
                    }
                    finally
                    {
                        // Stop the application once the work is done
                        _appLifetime.StopApplication();
                    }
                });
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _log.LogDebug($"Exiting with return code: {_exitCode}");

            // Exit code may be null if the user cancelled via Ctrl+C/SIGTERM
            Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
            return Task.CompletedTask;
        }
        
        
        /*
        public void StartAsync()
        {
            // request an instance of type ISomeService
            // from the ServicePipeline built
            // returns an object of type SomeService
            var deviceID = _deviceConfig.DeviceId;

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
        */
        
        private async void SolarListener_SolarMessageReceived(object sender, SolarMessageEventArgs e)
        {
            Console.WriteLine(e.Data.Day.Generated);
            var solarData = ConvertSolarToJson(e.Data);
            await _deviceClient.SendEventAsync(solarData);
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