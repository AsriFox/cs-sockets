namespace CsSockets
{
    public class UdpSender
    {
        readonly System.Net.Sockets.UdpClient sender = new() { EnableBroadcast = true, ExclusiveAddressUse = false };
        readonly System.Net.IPEndPoint endPoint;

        public UdpSender(string host, int port)
        {
            endPoint = new(System.Net.IPAddress.Parse(host), port);
        }

        ~UdpSender() => sender.Close();

        public System.Threading.Tasks.Task<int> Send(byte[] data) => sender.SendAsync(data, data.Length, endPoint);

        public System.Threading.Tasks.Task<int> Send(string message) => Send(Util.encoding.GetBytes(message));
    }
}
