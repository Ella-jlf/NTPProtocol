using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NTP_client
{
    class Program
    {
        public static DateTime GetNetworkTime()
        {
            //default Windows time server
            Console.WriteLine("Type local or network");
            string k = Console.ReadLine();
            IPEndPoint ipEndPoint;
            switch (k)
            {
                case "local":
                    {
                        ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7123);
                        break;
                    }
                default:
                    {
                        const string ntpServer = "time.windows.com";
                        var addresses = Dns.GetHostEntry(ntpServer).AddressList;
                        ipEndPoint = new IPEndPoint(addresses[0], 123);
                        break;
                    }

            }

            var ntpData = new byte[48];          
            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; // 00011011

           

            //The UDP port number assigned to NTP is 123
            
            //NTP uses UDP

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);

                //Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 6000;
                var time_1900 = new DateTime(1900, 1, 1, 1, 0, 0, DateTimeKind.Utc);
                ulong cur_time = (ulong)((DateTime.UtcNow-time_1900).TotalSeconds);
                cur_time = SwapEndianness(cur_time);
                var data = new byte[4];
                data = BitConverter.GetBytes(cur_time);
                for (int i = 0; i < data.Length; i++)
                    ntpData[24 + i] = data[i];
                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."

            var networkDateTime = HandleTime(ntpData,k);
            return networkDateTime.ToLocalTime();
        }
        public static DateTime HandleTime(byte[] data,string k)
        {
            ulong int_send = BitConverter.ToUInt32(data, 40);
            ulong fract_send = BitConverter.ToUInt32(data, 44);
            ulong int_recv = BitConverter.ToUInt32(data, 32);
            ulong fract_recv = BitConverter.ToUInt32(data, 36);
            ulong int_start = BitConverter.ToUInt32(data, 24);
            ulong fract_start = BitConverter.ToUInt32(data, 28);
            int_send = SwapEndianness(int_send);
            fract_send = SwapEndianness(fract_send);
            int_recv = SwapEndianness(int_recv);
            fract_recv = SwapEndianness(fract_recv);
            int_start = SwapEndianness(int_start);
            fract_start = SwapEndianness(fract_start);
            double miliseconds;
            switch (k)
            {
                case "local":
                    {
                        miliseconds = ((int_recv * 1000) + ((fract_recv * 1000) / 0x100000000L) - (int_start * 1000) + ((fract_start * 1000) / 0x100000000L)) + (int_send * 1000) + ((fract_send * 1000) / 0x100000000L);
                            
                        break;
                    }
                default:
                    {
                        miliseconds = (int_send * 1000) + ((fract_send * 1000) / 0x100000000L);
                        break;
                    }

            }
            
            var networkTime = new DateTime(1900, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(miliseconds);
       //     Console.WriteLine(new DateTime(1900, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((int_recv * 1000) + ((fract_recv * 1000) / 0x100000000L)));
       //     Console.WriteLine(new DateTime(1900, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((int_start * 1000) + ((fract_start * 1000) / 0x100000000L)));
            return networkTime;
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
            DateTime time = GetNetworkTime();
            Console.WriteLine(time.ToString());
            Console.ReadLine();
        }
    }
}
