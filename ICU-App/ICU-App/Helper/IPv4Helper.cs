using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ICU_App.Helper;

/// <summary>
/// This class provides helper methods for working with IPv4 addresses.
/// </summary>
static class IPv4Helper
{
    /// <summary>
    /// Returns the network address given an IP address and a subnetmask.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="subnetMask"></param>
    /// <returns>network address</returns>
    /// <exception cref="ArgumentException"></exception>
    public static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
    {
        byte[] ipAddressBytes = address.GetAddressBytes();
        byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

        if (ipAddressBytes.Length != subnetMaskBytes.Length)
        {
            throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
        }

        byte[] networkAddressBytes = new byte[ipAddressBytes.Length];
        for (int i = 0; i < networkAddressBytes.Length; i++)
        {
            networkAddressBytes[i] = (byte)(ipAddressBytes[i] & subnetMaskBytes[i]);
        }

        return new IPAddress(networkAddressBytes);
    }

    /// <summary>
    /// Returns the broadcast address given an IP address and a subnetmask.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="subnetMask"></param>
    /// <returns>broadcast address</returns>
    /// <exception cref="ArgumentException"></exception>
    public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
    {
        byte[] ipAddressBytes = address.GetAddressBytes();
        byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

        if (ipAddressBytes.Length != subnetMaskBytes.Length)
        {
            throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
        }

        byte[] broadcastAddressBytes = new byte[ipAddressBytes.Length];
        for (int i = 0; i < broadcastAddressBytes.Length; i++)
        {
            broadcastAddressBytes[i] = (byte)(ipAddressBytes[i] | ~subnetMaskBytes[i]);
        }
        return new IPAddress(broadcastAddressBytes);
    }

    /// <summary>
    /// Returns the IP address that depends on the given address and specified increment.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="increment"></param>
    /// <returns>next IP address</returns>
    public static IPAddress GetNextIPAddress(IPAddress address, uint increment)
    {
        byte[] addressBytes = address.GetAddressBytes().Reverse().ToArray();
        uint nextAddress = BitConverter.ToUInt32(addressBytes, 0) + increment;
        IPAddress ipAddress = new IPAddress(BitConverter.GetBytes(nextAddress).Reverse().ToArray());
        return ipAddress;
    }



}
