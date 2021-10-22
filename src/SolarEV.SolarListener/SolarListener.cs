using System.Diagnostics;
using System.Xml;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using SolarEV.Models;

namespace SolarEV.Services
{
    public class SolarListener : ISolarListener
    {
        private IPAddress multicastAddress;
        private int multicastPort;
        private Socket multicastSocket;
        private MulticastOption? multicastOption;
        private readonly ILogger<SolarListener> _log;


        public event EventHandler<SolarMessageEventArgs> SolarMessageReceived;

        public SolarListener(ILogger<SolarListener> log) => _log = log;

        private void StartMulticast()
        {
            try
            {
                multicastSocket = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Dgram,
                                         ProtocolType.Udp);
                IPAddress localIPAddr = IPAddress.Parse("10.0.0.168");
                _log.LogInformation("Local IP Address: " + localIPAddr.ToString());

                //IPAddress localIP = IPAddress.Any;
                EndPoint localEP = (EndPoint)new IPEndPoint(localIPAddr, multicastPort);

                multicastSocket.Bind(localEP);


                // Define a MulticastOption object specifying the multicast group
                // address and the local IPAddress.
                // The multicast group address is the same as the address used by the server.
                multicastOption = new MulticastOption(multicastAddress, localIPAddr);

                multicastSocket.SetSocketOption(SocketOptionLevel.IP,
                                            SocketOptionName.AddMembership,
                                            multicastOption);
            }

            catch (Exception e)
            {
                _log.LogError(exception:e, message:e.Message);
            }
        }

        private void ReceiveBroadcastMessages()
        {
            byte[] bytes;
            IPEndPoint groupEP = new IPEndPoint(multicastAddress, multicastPort);
            EndPoint remoteEP = (EndPoint)new IPEndPoint(IPAddress.Any, 0);


            while (true)
            {
                _log.LogInformation("Waiting for broadcast");
                _log.LogInformation(remoteEP.ToString());
                try
                {

                    while (true)
                    {
                        bytes = new Byte[1024];
                        multicastSocket.ReceiveFrom(bytes, ref remoteEP);

                        ProcessPacket(bytes);
                    }
                }
                catch (Exception e)
                {
                    _log.LogError(exception:e, message:e.Message);
                }
                finally
                {
                    multicastSocket.Close();
                    _log.LogInformation("Socket closed");
                    StartMulticast();
                }
            }
        }

        private void ProcessPacket(byte[] bytes)
        {
            MemoryStream doc = new(bytes);
            // HACK Ignore electricity price messages for now
            if (bytes[1] != 101)
            {
                //Console.WriteLine("Received broadcast from {0} :\n {1}\n", remoteEP.ToString(), text);
                try
                {
                    var solarSerializer = new XmlSerializer(typeof(Solar));
                    var solarData = (Solar)solarSerializer.Deserialize(doc);
                    Console.WriteLine($"Solar broadcast received: Generating {solarData.Current.Generating.Text} - Export {solarData.Current.Exporting.Text}");

                    SolarMessageReceived?.Invoke(this, new SolarMessageEventArgs(solarData));
                }
                catch (XmlException ex)
                {
                    doc.Position = 0;
                    var document = new XmlDocument();
                    document.Load(doc);
                    _log.LogError($"Second byte is: {bytes[1]}");
                    _log.LogError(ex, document.OuterXml);
                }
            }
        }

        public Task StartListeningAsync()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        multicastAddress = IPAddress.Parse("224.192.32.19");
                        multicastPort = 22600;

                        // Start a multicast group.
                        StartMulticast();

                        Console.WriteLine("Current multicast group is: " + multicastOption.Group);
                        Console.WriteLine("Current multicast local address is: " + multicastOption.LocalAddress);

                        // Receive broadcast messages.
                        ReceiveBroadcastMessages();
                    }
                    catch (Exception e)
                    {
                        _log.LogCritical(exception:e, message:e.Message);
                    }
                }
            });
        }

        public Task StopListeningAsync()
        {
            throw new NotImplementedException();
        }
    }
}