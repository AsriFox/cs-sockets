using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CsSocketServer
{
    internal class UdpListener : CsSockets.UdpBase
    {
        private IPEndPoint _listenOn;

        // public UdpListener() : this(new IPEndPoint(IPAddress.Any, 32123)) { }

        public UdpListener(IPEndPoint endPoint)
        {
            _listenOn = endPoint;
            Client = new(_listenOn);
        }

        public void Reply(string message, IPEndPoint endPoint)
        {
            var datagram = Encod.GetBytes(message);
            Client.Send(datagram, datagram.Length, endPoint);
        }
    }
}
