using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsSocketClient
{
    internal class UdpUser : CsSockets.UdpBase
    {
        private UdpUser() {}

        public static UdpUser ConnectTo(string hostName, int port)
        {
            UdpUser connection = new();
            connection.Client.Connect(hostName, port);
            return connection;
        }

        public void Send(string message)
        {
            var datagram = Encod.GetBytes(message);
            Client.Send(datagram, datagram.Length);
        }
    }
}
