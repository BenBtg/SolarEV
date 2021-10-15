﻿using System;
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
      ISolarListener listener = new SolarListener();
      using IHost host = CreateHostBuilder(args).Build();       
      await host.RunAsync();     
      ISolarListener solarListener = host.Services.GetService<ISolarListener>();
      IIoTDeviceClientService deviceClientService = host.Services.GetService<IIoTDeviceClientService>();
      await deviceClientService.ConnectAsync();
      await solarListener.StartListeningAsync();
      solarListener.SolarMessageReceived += SolarListener_SolarMessageReceived;
      Console.WriteLine("End");
    }

    private static void SolarListener_SolarMessageReceived(object sender, SolarMessageEventArgs e)
    {
      Console.WriteLine(e.Data.Day.Generated);
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
          .ConfigureServices((_, services) =>
              services.AddSingleton<ISolarListener, SolarListener>()
                      .AddSingleton<IIoTDeviceClientService, IoTDeviceClientService>()
                      .AddSingleton<IDeviceConfigService, DeviceConfigService>());
  }
}