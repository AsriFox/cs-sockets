namespace CsSockets
{
    public struct Received
    {
        public System.Net.IPEndPoint Sender;
        public string Message;
    }

    public abstract class UdpBase
    {
        public static readonly System.Text.Encoding Encod = System.Text.Encoding.Unicode;

        protected System.Net.Sockets.UdpClient Client = new();

        // public async Task<Received> Receive()
        public Received Receive()
        {
            //var result = await Client.ReceiveAsync();
            var t = Client.ReceiveAsync();
            t.Wait();
            return new Received {
                Sender = t.Result.RemoteEndPoint,
                Message = Encod.GetString(t.Result.Buffer)
            };
        }
    }
}
