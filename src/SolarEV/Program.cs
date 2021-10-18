using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarEV.IoT;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Serilog;
using SolarEV.IoT.Models;
using SolarEV.Services;

namespace SolarEV
{
    internal sealed class Program
    {
        public static async Task Main(string[] args)
        {
            var host = AppStartup();

            await host.RunConsoleAsync();
        }
        
        static IHostBuilder AppStartup()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            // Specifying the configuration for serilog
            Log.Logger = new LoggerConfiguration() // initiate the logger configuration
                .ReadFrom.Configuration(builder.Build()) // connect serilog to our configuration folder
                .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog 
                .WriteTo.Console() // decide where the logs are going to be shown
                .CreateLogger(); //initialise the logger

            Log.Logger.Information("Application Starting");

            var host = Host.CreateDefaultBuilder() // Initialising the Host 
                .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .ConfigureServices((hostContext, services) =>
                {
                    // Adding the DI container for configuration
//                    services.AddSingleton<IConfiguration>(_configuration);
                    services.AddHostedService<ConsoleHostedService>()
                            .AddSingleton<ISolarListener, SolarListener>()
                            .AddSingleton<IIoTDeviceClientService, IoTDeviceClientService>();
                    
                    services.AddOptions<DeviceConfig>().Bind(hostContext.Configuration.GetSection("DeviceConfig"));
                });

            return host;
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT");
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

        }
    }
}