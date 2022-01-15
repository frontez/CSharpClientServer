using System;
using System.Collections.Generic;
using System.Text;

namespace Bindings
{
    //get send from server to client
    //client has to listen to serverpackets
    public enum ServerPackets
    {
        SConnectionOK = 1,
        SInfo = 2,
        SDisconnect = 3,
        SNewConnection = 4,
    }

    //get send from client to server
    //server has to listen to clientpackets
    public enum ClientPackets
    {
        CThankYou = 1,
        CInfo     = 2,
        CDisconnect = 3,
        CNewConnection = 4,
    }
}
