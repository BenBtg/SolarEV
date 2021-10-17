using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SolarEV.IoT;
using SolarEV.Services;

namespace SolarEV
{
    internal sealed class ConsoleHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ISolarListener _solarListener;
        private readonly IDeviceConfig _deviceConfig;

        private int? _exitCode;

        public ConsoleHostedService(
            ILogger<ConsoleHostedService> logger,
            IHostApplicationLifetime appLifetime,
            ISolarListener solarListener,
            IDeviceConfig deviceConfig)
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _solarListener = solarListener;
            _deviceConfig = deviceConfig;
            
            _solarListener.SolarMessageReceived += SolarListener_SolarMessageReceived;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Starting with arguments: {string.Join(" ", Environment.GetCommandLineArgs())}");

            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        
                            
                        await _solarListener.StartListeningAsync();

                        while (true)
                        {
                            
                        }
                        
                   
                        _logger.LogInformation($"{DateTime.Today.AddDays(i).DayOfWeek}: {temperatures[i]}");
                   

                        _exitCode = 0;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception!");
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
            _logger.LogDebug($"Exiting with return code: {_exitCode}");

            // Exit code may be null if the user cancelled via Ctrl+C/SIGTERM
            Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
            return Task.CompletedTask;
        }
        
        
        public void StartAsync()
        {
            // request an instance of type ISomeService
            // from the ServicePipeline built
            // returns an object of type SomeService
            
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