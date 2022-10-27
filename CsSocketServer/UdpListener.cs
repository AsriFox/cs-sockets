namespace CsSocketServer
{
    using System.Net;

    internal class UdpListener : CsSockets.UdpBase
    {
        private readonly IPEndPoint listenOn;
        private readonly IPEndPoint replyOn;

        public UdpListener(IPAddress host, int portListen, int portReply)
        {
            listenOn = new(host, portListen);
            replyOn = new(host, portReply);
            Client = new(listenOn);
        }

        public void Reply(byte[] message, IPEndPoint endPoint = null)
        {
            Client.Send(message, message.Length, endPoint ?? replyOn);
        }
    }
}
