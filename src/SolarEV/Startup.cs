using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolarEV.IoT;
using SolarEV.Services;

namespace SolarEV
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _provider;

        // access the built service pipeline
        public IServiceProvider Provider => _provider;

// access the built configuration
        public IConfiguration Configuration => _configuration;

        public Startup()
        {
            var environment = Environment.GetEnvironmentVariable("SOLAREV_ENVIRONMENT");

            _configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{environment}.json", optional: true)
                            .AddEnvironmentVariables()
                            .Build();

            // instantiate
            var services = new ServiceCollection();

            // add necessary services
            services.AddSingleton<IConfiguration>(_configuration);
            services.AddSingleton<ISolarListener, SolarListener>();
            services.AddSingleton<IDeviceConfigService, DeviceConfigService>();
            services.AddSingleton<IIoTDeviceClientService, IoTDeviceClientService>();

            // build the pipeline
            _provider = services.BuildServiceProvider();
        }
    }
}