namespace CsSockets
{
    using System.Net;
    using System.Net.Sockets;

    public class UdpReceiver
    {
        readonly UdpClient receiver;

        /// <summary>
        /// Create a receiver that listens on only one address (use in clients).
        /// </summary>
        public UdpReceiver(string host, int port)
        {
            receiver = new();
            receiver.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            receiver.Connect(host, port);
        }

        /// <summary>
        /// Create a receiver that listens on all addresses (use in servers).
        /// </summary>
        public UdpReceiver(int port)
        {
            receiver = new(new IPEndPoint(IPAddress.Any, port));
            receiver.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        }

        ~UdpReceiver() => receiver.Close();

        public byte[] Receive(ref IPEndPoint remoteEp) => receiver.Receive(ref remoteEp);
    }
}
