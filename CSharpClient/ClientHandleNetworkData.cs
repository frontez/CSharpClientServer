using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bindings;

namespace CSharpClient
{
    class ClientHandleNetworkData
    {
        private delegate void Packet_(byte[] data);
        private static Dictionary<int, Packet_> Packets;

        public static void InitializeNetworkPackages()
        {
            Console.WriteLine("Initialize Network Packages");
            Packets = new Dictionary<int, Packet_>
            {
                {(int)ServerPackets.SConnectionOK, HandleConnectionOK },
                {(int)ServerPackets.SInfo, HandleInfo },
                {(int)ServerPackets.SDisconnect, HandleDisconnect },
                {(int)ServerPackets.SNewConnection, HandleNewConnection },
            };
        }

        public static void HandleNetworkInformation(byte[] data)
        {
            int packetnum;
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            packetnum = buffer.ReadInteger();
            buffer.Dispose();
            if(Packets.TryGetValue(packetnum, out Packet_ Packet))
            {
                Packet.Invoke(data);
            }
        }

        private static void HandleConnectionOK(byte[] data)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            int packetnum = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //add your code you wnat to execute here:
            Console.WriteLine(msg);

            ClientTCP.ThankYouServer();
        }

        private static void HandleInfo(byte[] data)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            int packetnum = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //add your code you wnat to execute here:
            Console.WriteLine(msg);
        }


        public static void HandleDisconnect(byte[] data)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            int packetnum = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //add your code you wnat to execute here:
            Console.WriteLine(msg);
            ClientTCP.DisconenctFromServer();
        }

        private static void HandleNewConnection(byte[] data)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteBytes(data);
            int packetnum = buffer.ReadInteger();
            string msg = buffer.ReadString();
            buffer.Dispose();

            //add your code you wnat to execute here:
            Console.WriteLine(msg);
        }
    }
}
