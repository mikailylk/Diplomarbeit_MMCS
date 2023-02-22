using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ICU_App.Helper;

/// <summary>
/// Represents the received message (including endpoint of sender).
/// </summary>
public struct Received
{
    public IPEndPoint Sender;
    public string Message;
}

/// <summary>
/// This class is a base class and defines common functionalities for
/// UDP listeners and clients
/// </summary>
abstract class UDPBase
{
    protected UdpClient Client;

    public CancellationTokenSource cancellationTokenSource;

    protected UDPBase()
    {
        Client = new UdpClient();
        cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Receives a message asynchronously and returns the received message and sender information.
    /// </summary>
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

/// <summary>
/// A UDP listener (Server) that listens on a specific IP endpoint for incoming messages.
/// </summary>
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

    /// <summary>
    /// Sends a reply message to the specified endpoint.
    /// </summary>
    public void Reply(string message, IPEndPoint endPoint)
    {
        var datagram = Encoding.ASCII.GetBytes(message);
        Client.Send(datagram, datagram.Length, endPoint);
    }

    /// <summary>
    /// Closes the UDP listener.
    /// </summary>
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

/// <summary>
/// A UDP client that connects to a remote host and sends messages to it.
/// </summary>
class UDPClient : UDPBase
{
    private UDPClient() { }

    /// <summary>
    /// Connects to the specified host and port and returns a new UDPClient object.
    /// </summary>
    public static UDPClient ConnectTo(string hostname, int port)
    {
        var connection = new UDPClient();
        connection.Client.Connect(hostname, port);
        return connection;
    }

    /// <summary>
    /// Sends a message to the connected host and port.
    /// </summary>
    public void Send(string message)
    {
        var datagram = Encoding.ASCII.GetBytes(message);
        Client.Send(datagram, datagram.Length);
    }
}



