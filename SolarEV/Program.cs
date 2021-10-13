using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SolarEV.Services;

namespace SolarEV.TransportProtocols.Utilities
{
  public class SolarEV
  {
   
    public static void Main(String[] args)
    {
      ISolarListener listener = new SolarListener();
      
    }
  }
}