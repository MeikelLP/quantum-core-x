using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Serilog;

namespace QuantumCore.Core.Utils
{
    public static class IpUtils
    {
        public static IPAddress PublicIP { get; set; } 
        
        public static int ConvertIpToUInt(string ip)
        {
            return ConvertIpToUInt(IPAddress.Parse(ip));
        }

        public static int ConvertIpToUInt(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            var ret = (int) bytes[3] << 24;
            ret += (int) bytes[2] << 16;
            ret += (int) bytes[1] << 8;
            ret += (int) bytes[0];
            return ret;
        }

        public static void SearchPublicIp()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(c =>
                    c.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    c.OperationalStatus == OperationalStatus.Up);
            foreach (var iface in interfaces)
            {
                var ip = iface.GetIPProperties().UnicastAddresses
                    .Where(c => c.Address.AddressFamily == AddressFamily.InterNetwork).Select(c => c.Address)
                    .FirstOrDefault();

                if (ip != null)
                {
                    PublicIP = ip;
                    Log.Debug($"Public IP is {PublicIP}");
                    return;
                }
            }

            Log.Warning($"Failed to look up public ip!");
            PublicIP = IPAddress.Loopback;
        }
    }
}