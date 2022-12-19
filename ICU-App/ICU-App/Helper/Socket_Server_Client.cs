using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ICU_App.Helper;


public struct Received
{
    public IPEndPoint Sender;
    public string Message;
}

abstract class UDPBase
{
    protected UdpClient Client;

    public CancellationTokenSource cancellationTokenSource;

    protected UDPBase()
    {
        Client = new UdpClient();
        cancellationTokenSource = new CancellationTokenSource();
    }


    public async Task<Received> Receive()
    {
        try
        {
            var result = await Client.ReceiveAsync(cancellationTokenSource.Token);
            return new Received()
            {
                Message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length),
                Sender = result.RemoteEndPoint
            };
        }
        catch (OperationCanceledException)
        {
            return new Received();
        }

    }
}

// Server
class UDPListener : UDPBase
{
    private IPEndPoint _listOn;

    public UDPListener() : this(new IPEndPoint(IPAddress.Any, 8086))
    { }

    public UDPListener(IPEndPoint listOn)
    {
        _listOn = listOn;
        Client = new UdpClient(listOn);
    }

    public void Reply(string message, IPEndPoint endPoint)
    {
        var datagram = Encoding.ASCII.GetBytes(message);
        Client.Send(datagram, datagram.Length, endPoint);
    }

    public void Close()
    {
        try
        {
            Client.Close();
        }
        catch (Exception)
        { }
    }
}

//Client
class UDPClient : UDPBase
{
    private UDPClient() { }

    public static UDPClient ConnectTo(string hostname, int port)
    {
        var connection = new UDPClient();
        connection.Client.Connect(hostname, port);
        return connection;
    }

    public void Send(string message)
    {
        var datagram = Encoding.ASCII.GetBytes(message);
        Client.Send(datagram, datagram.Length);
    }

}



