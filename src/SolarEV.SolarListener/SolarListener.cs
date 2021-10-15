using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SolarEV.Models;

namespace SolarEV.Services
{
    public class SolarListener : ISolarListener
    {
        private static IPAddress multicastAddress;
        private static int multicastPort;
        private static Socket multicastSocket;
        private static MulticastOption multicastOption;

        public event EventHandler<SolarMessageEventArgs> SolarMessageReceived;

        public SolarListener()
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

        private static void StartMulticast()
        {
            try
            {
                multicastSocket = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Dgram,
                                         ProtocolType.Udp);

                IPAddress localIPAddr = IPAddress.Parse("10.0.0.168");

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
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveBroadcastMessages()
        {
            byte[] bytes;
            IPEndPoint groupEP = new IPEndPoint(multicastAddress, multicastPort);
            EndPoint remoteEP = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

            XmlSerializer solarSerializer = new XmlSerializer(typeof(Solar));

            while (true)
            {
                Console.WriteLine("Waiting for multicast packets.......");
                try
                {
                    bytes = new Byte[1024];
                    multicastSocket.ReceiveFrom(bytes, ref remoteEP);

                    //var text = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    // HACK Ignore electricity price messages for now
                    if (bytes[1] == 101)
                        return;

                    //Console.WriteLine("Received broadcast from {0} :\n {1}\n", remoteEP.ToString(), text);

                    MemoryStream doc = new MemoryStream(bytes);
                    // var document = new XmlDocument();
                    // document.Load(doc);
                    // Console.WriteLine(document.OuterXml);

                    var solarData = (Solar)solarSerializer.Deserialize(doc);
                    Console.WriteLine($"Solar broadcast received: Generating {solarData.Current.Generating.Text} - Export {solarData.Current.Exporting.Text}");

                    //   Console.WriteLine(document.SelectSingleNode("electricity").InnerText);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    multicastSocket.Close();
                    StartMulticast();
                }
            }
        }

        internal void StartListening()
        {
            throw new NotImplementedException();
        }

        public Task StartListeningAsync()
        {
            throw new NotImplementedException();
        }

        public Task StopListeningAsync()
        {
            throw new NotImplementedException();
        }
    }
}