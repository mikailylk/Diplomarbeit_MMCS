using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ICU_App.Helper;

static class IPv4Helper
{
    public static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
    {
        byte[] ipAdressBytes = address.GetAddressBytes();
        byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

        if (ipAdressBytes.Length != subnetMaskBytes.Length)
        {
            throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
        }

        byte[] networkAddressBytes = new byte[ipAdressBytes.Length];
        for (int i = 0; i < networkAddressBytes.Length; i++)
        {
            networkAddressBytes[i] = (byte)(ipAdressBytes[i] & subnetMaskBytes[i]);
        }

        return new IPAddress(networkAddressBytes);
    }

    public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
    {
        byte[] ipAdressBytes = address.GetAddressBytes();
        byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

        if (ipAdressBytes.Length != subnetMaskBytes.Length)
        {
            throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
        }

        byte[] broadcastAddressBytes = new byte[ipAdressBytes.Length];
        for (int i = 0; i < broadcastAddressBytes.Length; i++)
        {
            broadcastAddressBytes[i] = (byte)(ipAdressBytes[i] | ~subnetMaskBytes[i]);
        }
        return new IPAddress(broadcastAddressBytes);
    }

    public static IPAddress GetNextIPAddress(IPAddress address, uint increment)
    {
        byte[] addressBytes = address.GetAddressBytes().Reverse().ToArray();
        uint nextAddress = BitConverter.ToUInt32(addressBytes, 0) + increment;
        IPAddress ipAddress = new IPAddress(BitConverter.GetBytes(nextAddress).Reverse().ToArray());
        return ipAddress;
    }



}
