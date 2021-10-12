using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

// This is the listener example that shows how to use the MulticastOption class.
// In particular, it shows how to use the MulticastOption(IPAddress, IPAddress)
// constructor, which you need to use if you have a host with more than one
// network card.
// The first parameter specifies the multicast group address, and the second
// specifies the local address of the network card you want to use for the data
// exchange.
// You must run this program in conjunction with the sender program as
// follows:
// Open a console window and run the listener from the command line.
// In another console window run the sender. In both cases you must specify
// the local IPAddress to use. To obtain this address run the ipconfig command
// from the command line.
//
namespace Mssc.TransportProtocols.Utilities
{

  public class TestMulticastOption
  {

    private static IPAddress mcastAddress;
    private static int mcastPort;
    private static Socket mcastSocket;
    private static MulticastOption mcastOption;


    private static void MulticastOptionProperties()
    {
      Console.WriteLine("Current multicast group is: " + mcastOption.Group);
      Console.WriteLine("Current multicast local address is: " + mcastOption.LocalAddress);
    }


    private static void StartMulticast()
    {
    
      try
      {
        mcastSocket = new Socket(AddressFamily.InterNetwork,
                                 SocketType.Dgram,
                                 ProtocolType.Udp);
        
        IPAddress localIPAddr = IPAddress.Parse("10.0.0.168");

        //IPAddress localIP = IPAddress.Any;
        EndPoint localEP = (EndPoint)new IPEndPoint(localIPAddr, mcastPort);

        mcastSocket.Bind(localEP);


        // Define a MulticastOption object specifying the multicast group
        // address and the local IPAddress.
        // The multicast group address is the same as the address used by the server.
        mcastOption = new MulticastOption(mcastAddress, localIPAddr);

        mcastSocket.SetSocketOption(SocketOptionLevel.IP,
                                    SocketOptionName.AddMembership,
                                    mcastOption);
      }

      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
    }

    private static void ReceiveBroadcastMessages()
    {
      bool done = false;
      byte[] bytes = new Byte[1024];
      IPEndPoint groupEP = new IPEndPoint(mcastAddress, mcastPort);
      EndPoint remoteEP = (EndPoint) new IPEndPoint(IPAddress.Any,0);
    
      while(true)
      {
      try
      {
        while (!done)
        {
          Console.WriteLine("Waiting for multicast packets.......");
          Console.WriteLine("Enter ^C to terminate.");

          mcastSocket.ReceiveFrom(bytes, ref remoteEP);


          var text = Encoding.ASCII.GetString(bytes,0,bytes.Length);

          //Console.WriteLine("Received broadcast from {0} :\n {1}\n", groupEP.ToString(), text);

        var document = new XmlDocument();         
        document.LoadXml(text);
        Console.WriteLine(document.OuterXml);

     //   Console.WriteLine(document.SelectSingleNode("electricity").InnerText);
            
        }

        
      }

      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
      finally
      {
        mcastSocket.Close();
        StartMulticast();
      }
      }
    }

    public static void Main(String[] args)
    {
      mcastAddress = IPAddress.Parse("224.192.32.19");
      mcastPort = 22600;

      // Start a multicast group.
      StartMulticast();

      // Display MulticastOption properties.
      MulticastOptionProperties();

      // Receive broadcast messages.
      ReceiveBroadcastMessages();
    }
  }
}