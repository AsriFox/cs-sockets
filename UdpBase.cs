namespace CsSockets
{
    using System.Net;
    using System.Net.Sockets;

    public struct Received
    {
        public IPEndPoint Sender;
        public byte[] Datagram;
    }

    public abstract class UdpBase
    {
        public static readonly System.Text.Encoding Encod = System.Text.Encoding.Unicode;

        protected UdpClient Client = new();

        // public async Task<Received> Receive()
        public Received Receive(bool timeout = false)
        {
            //var result = await Client.ReceiveAsync();
            var t = Client.BeginReceive(null, null);
            if (timeout)
                t.AsyncWaitHandle.WaitOne(System.TimeSpan.FromSeconds(1));
            else t.AsyncWaitHandle.WaitOne();

            if (t.IsCompleted) {
                IPEndPoint remote = null;
                byte[] data = Client.EndReceive(t, ref remote);
                return new Received {
                    Sender = remote,
                    Datagram = data
                };
            }
            else throw new SocketException((int)SocketError.TimedOut);
        }
    }
}
