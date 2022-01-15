using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Bindings;

namespace CSharpClient
{
    class ClientTCP
    {
        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private byte[] _asyncbuffer = new byte[1024];
        private static bool isServerDead = false;


        public static void ConnectToServer()
        {
            Console.WriteLine("Connecting to server...");
            _clientSocket.BeginConnect("127.0.0.1", 5555, new AsyncCallback(ConnectCallback), _clientSocket);
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            _clientSocket.EndConnect(ar);
            while (true)
            {
                OnRecieve();
            }
        }
        private static void OnRecieve()
        {
            if (isServerDead) return;

            byte[] _sizeinfo = new byte[4];
            byte[] _recievedbuffer = new byte[1024];

            int totalread = 0, currentread = 0;

            try
            {
                currentread = totalread = _clientSocket.Receive(_sizeinfo);
                if (totalread <= 0)
                {
                    Console.WriteLine("You are not connected to the server.");
                }
                else
                {
                    while (totalread < _sizeinfo.Length && currentread > 0)
                    {
                        currentread = _clientSocket.Receive(_sizeinfo, totalread, _sizeinfo.Length - totalread, SocketFlags.None);
                        totalread += currentread;
                    }

                    int messagesize = 0;
                    messagesize |= _sizeinfo[0];
                    messagesize |= (_sizeinfo[1] << 8);
                    messagesize |= (_sizeinfo[2] << 18);
                    messagesize |= (_sizeinfo[3] << 24);

                    byte[] data = new byte[messagesize];

                    totalread = 0;
                    currentread = totalread = _clientSocket.Receive(data, totalread, data.Length - totalread, SocketFlags.None);
                    while(totalread < messagesize && currentread > 0)
                    {
                        currentread = _clientSocket.Receive(data, totalread, data.Length - totalread, SocketFlags.None);
                        totalread += currentread;
                    }

                    ClientHandleNetworkData.HandleNetworkInformation(data);
                }
            }
            catch
            {
                Console.WriteLine("You are not connected to the server.");
            }
        }

        public static void SendData (byte[] data)
        {
            _clientSocket.Send(data);
        }

        public static void ThankYouServer()
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteInteger((int)ClientPackets.CInfo);
            buffer.WriteString("Thank you bruv, for letting me connect ya server.");
            SendData(buffer.ToArray());
            buffer.Dispose();
        }

        public static void DisconenctFromServer()
        {
            isServerDead = true;
            _clientSocket.Close();
            Console.WriteLine("Connection has been closed.");
        }
    }
}
