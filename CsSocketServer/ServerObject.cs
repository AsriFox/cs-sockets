namespace CsSocketServer
{
    internal class ServerObject
	{
        public event System.Action<string> Write;

        private readonly UdpListener listener;
        private readonly System.Threading.Thread listenThread;

        public ServerObject(string host, int port)
        {
            listener = new(
                System.Net.IPAddress.Parse(host), 
                port,
                port
            );
            listenThread = new(Listen);
            listenThread.Start();
        }

        protected void Listen()
		{
			try {
				while (true) {
					var received = listener.Receive();
					//var idr = new System.Guid(received.Datagram[..16]);
                    string message = CsSockets.UdpBase.Encod.GetString(received.Datagram[16..]);
                    Write?.Invoke(message);
                    listener.Reply(received.Datagram);
				}
			}
			catch (System.Exception e) {
                Write?.Invoke(e.Message);
                System.Environment.Exit(0);
            }
		}

        //public void Dispose()
        //{
        //    if (listener is null) return;
        //    // Notify users about the server shutdown:
        //    BroadcastMessage(EncodeMessage(Guid.Empty, "$serverShutdown"));
        //}
	}
}
