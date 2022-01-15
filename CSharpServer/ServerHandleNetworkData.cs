using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bindings; 

namespace CSharpServer
{
    class ServerHandleNetworkData
    {
        private delegate void Packet_(int index, byte[] data);
        private static Dictionary<int, Packet_> Packets;

        public static void InitializeNetworkPackages()
        {
            Console.WriteLine("Initialize Network Packages");
            Packets = new Dictionary<int, Packet_>
            {
                {(int)ClientPackets.CThankYou, HandleThankYou },
                {(int)ClientPackets.CInfo, HandleInfo }

            };
        }

        public static void HandleNetworkInformation(int index, byte[] data)
        {
            int packetnum;
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadInteger();
            buffer.Dispose();
            if (Packets.TryGetValue(packetnum, out Packet_ Packet))
            {
                Packet.Invoke(index, data);
            }
        }

        private static void HandleThankYou(int index, byte[] data)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            int packetnum = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //add your code you wnat to execute here:
            Console.WriteLine(msg);
        }

        private static void HandleInfo(int index, byte[] data)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            int packetnum = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //add your code you wnat to execute here:
            Console.WriteLine(msg);
            ServerTCP.SendInfo(index, "You sent this: " + msg);
        }
    }
}
