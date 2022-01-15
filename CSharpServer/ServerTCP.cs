using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Bindings;

namespace CSharpServer
{
    class ServerTCP
    {
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static byte[] _buffer = new byte[1024];

        public static Client[] _clients = new Client[Constants.MAX_PLAYERS];

        public static void SetupServer()
        {
            for (int i = 0; i < Constants.MAX_PLAYERS; i++)
            {
                _clients[i] = new Client();
            }
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 5555));
            _serverSocket.Listen(10);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket socket = _serverSocket.EndAccept(ar);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

            for (int i=0; i<Constants.MAX_PLAYERS; i++)
            {
                if (_clients[i].socket == null)
                {
                    _clients[i].socket = socket;
                    _clients[i].index = i;
                    _clients[i].ip = socket.RemoteEndPoint.ToString();
                    _clients[i].StartClient();
                    Console.WriteLine("Connection from '{0}' recieved", _clients[i].ip);
                    SendAllNewConnection(i); // SendConnectionOK(i);
                    return;
                }
            }
        }

        public static void SendDataTo(int index, byte[] data)
        {
            byte[] sizeinfo = new byte[4];
            sizeinfo[0] = (byte)data.Length;
            sizeinfo[1] = (byte)(data.Length >> 8);
            sizeinfo[2] = (byte)(data.Length >> 16);
            sizeinfo[3] = (byte)(data.Length >> 24);

            _clients[index].socket.Send(sizeinfo);
            _clients[index].socket.Send(data);
        }

        public static void SendConnectionOK(int index)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteInteger((int)ServerPackets.SConnectionOK);
            buffer.WriteString("You are succesfully connected to the server.");
            SendDataTo(index, buffer.ToArray());
            buffer.Dispose(); 
        }

        public static void SendAllNewConnection(int index)
        {
            string newClientIp = _clients[index].ip;

            for (int i = 0; i < Constants.MAX_PLAYERS; i++)
            {
                if (_clients[i].socket != null)
                {
                    if (i == index)
                    {
                        PacketBuffer buffer = new PacketBuffer();
                        buffer.WriteInteger((int)ServerPackets.SConnectionOK);
                        buffer.WriteString("You are succesfully connected to the server.");
                        SendDataTo(i, buffer.ToArray());
                        buffer.Dispose();
                    }
                    else
                    {
                        PacketBuffer buffer = new PacketBuffer();
                        buffer.WriteInteger((int)ServerPackets.SNewConnection);
                        buffer.WriteString(String.Format("Client {0} has connected to the server.", newClientIp));
                        SendDataTo(i, buffer.ToArray());
                        buffer.Dispose();
                    }
                }
            }
        }



        public static void SendInfo(int index, string info)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteInteger((int)ServerPackets.SInfo);
            buffer.WriteString(info);
            SendDataTo(index, buffer.ToArray());
            buffer.Dispose();
        }

        public static void SendDisconnect(int index, string info)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.WriteInteger((int)ServerPackets.SDisconnect);
            buffer.WriteString(info);
            SendDataTo(index, buffer.ToArray());
            buffer.Dispose();
        }

        public static void CloseAllClients()
        {
            for (int i = 0; i < Constants.MAX_PLAYERS; i++)
            {
                if (_clients[i].socket != null)
                {
                    _clients[i].DisconnectClientRemotely(i);
                }
            }
        }
    }

    class Client
    {
        public int index;
        public string ip;
        public Socket socket;
        public bool closing = false;
        private byte[] _buffer = new byte[1024];

        public void StartClient()
        {
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
            closing = false;
        }

        private void RecieveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            try
            {
                if (closing) return;
                int recieved = socket.EndReceive(ar);
                if (recieved <= 0)
                {
                    CloseClient(index);
                }
                else
                {
                    byte[] databuffer = new byte[recieved];
                    Array.Copy(_buffer, databuffer, recieved);
                    ServerHandleNetworkData.HandleNetworkInformation(index, databuffer);
                    socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallback), socket);
                }
            }
            catch
            {
                CloseClient(index);
            }
        }

        private void CloseClient(int index)
        {
            closing = true;
            Console.WriteLine("Connection from {0} has been terminated.", ip);
            //PlayerLeftGame
            this.socket.Close();
            this.socket = null;
            this.index = 0;
            this.ip = null; 
        }

        public void DisconnectClientRemotely(int index)
        {
            ServerTCP.SendDisconnect(index, "Server is shutting down...");

        }
    }
}
