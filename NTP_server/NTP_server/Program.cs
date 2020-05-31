using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace NTP_server
{
    class Program
    {
        public static void StartServer()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7123);
            IPEndPoint client = null;
            UdpClient server = new UdpClient(ep);
            var buff = new byte[48];
            var time_1900 = new DateTime(1900, 1, 1, 1, 0, 0, DateTimeKind.Utc);
            while (true)
            {
               
                buff = server.Receive(ref client);
                Console.WriteLine("new request");
                ulong recv_sec = (ulong)Math.Truncate((DateTime.UtcNow - time_1900).TotalSeconds);
                recv_sec = SwapEndianness(recv_sec);
                var data = new byte[4];
                data = BitConverter.GetBytes(recv_sec);
                for (int i = 0; i < data.Length; i++)
                    buff[32 + i] = data[i];
                buff[0] =0x1C; //00011100
                ulong send_sec = (ulong)Math.Truncate((DateTime.UtcNow - time_1900).TotalSeconds);
                send_sec = SwapEndianness(send_sec);
                data = new byte[4];
                data = BitConverter.GetBytes(send_sec);
                for (int i = 0; i < data.Length; i++)
                    buff[40 + i] = data[i];
                server.Send(buff, buff.Length, client);
                client = null;
            }
        }
        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
        static void Main()
        {
            StartServer();
        }
    }
}
