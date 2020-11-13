using System.Net;

namespace QuantumCore.Core.Utils
{
    public class IpUtils
    {
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
    }
}