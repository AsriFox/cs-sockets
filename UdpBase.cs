using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CsSockets
{
    public struct Received
    {
        public IPEndPoint Sender;
        public string Message;
    }

    public abstract class UdpBase
    {
        public static readonly Encoding Encod = Encoding.Unicode;

        protected UdpClient Client = new();

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
