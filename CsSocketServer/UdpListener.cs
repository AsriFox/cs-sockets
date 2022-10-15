namespace CsSocketServer
{
    using System.Net;

    internal class UdpListener : CsSockets.UdpBase
    {
        // private IPEndPoint _listenOn;

        public UdpListener(IPEndPoint endPoint)
        {
            // _listenOn = endPoint;
            // Client = new(_listenOn);
            Client = new(endPoint);
        }

        public void Reply(string message, IPEndPoint endPoint)
        {
            var datagram = Encod.GetBytes(message);
            Client.Send(datagram, datagram.Length, endPoint);
        }
    }
}
