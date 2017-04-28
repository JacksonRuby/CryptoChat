using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace CryptoChat
{
    public class ClientThread
    {
        private int id;
        private TcpClient clientConnection;
        //NetworkStream
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public TcpClient ClientConnection
        {
            get { return clientConnection; }
            set { clientConnection = value; }
        }


        public ClientThread(int clientId, TcpClient theNewClientConnection)
        {
            id = clientId;
            clientConnection = theNewClientConnection;

        }
    }
}
