namespace CsSocketClient
{
	using System;
	using System.Linq;

	internal class ClientObject : IDisposable
	{
        private readonly System.Threading.ManualResetEventSlim stopEvent = new(false);
        private readonly string _host;
        private readonly int _port;

        public event Action<string> MessageReceived;
		public event Action<string, bool> Disconnected;

        private UdpUser client;
        public Guid Id { get; private set; } = Guid.Empty;
        public string UserName { get; private set; }

        public ClientObject(string host, int port, string userName) 
		{
			_host = host;
			_port = port;
			client = UdpUser.ConnectTo(host, port);

			UserName = userName;
		}

		public void Start()
		{
			if (stopEvent.IsSet) {
				Disconnected?.Invoke("The thread is already running!", false);
				return;
			}

			// Notify the server of the new user joining:
            client.Send(EncodeMessage("$userJoin " + UserName));

			// Receive the assigned GUID from the server:
            var response = client.Receive();
            Id = new Guid(response.Datagram[..16]);
            string message = CsSockets.UdpBase.Encod.GetString(response.Datagram[16..]);

			// Print the greeting:
            MessageReceived?.Invoke(message);

			// Start receiving messages:
            System.Threading.Thread receiveThread = new(ReceiveMessages);
			stopEvent.Set();
			receiveThread.Start();
		}

		public void Stop() => stopEvent.Reset();

		public void WaitForConnection() {
            System.Threading.Thread waitThread = new(TryReconnecting);
			waitThread.Start();
			stopEvent.Wait();
		}

		private void TryReconnecting()
		{
			while (!stopEvent.IsSet) {
                System.Threading.Thread.Sleep(1000);
				try {
					client = UdpUser.ConnectTo(_host, _port);
                    Start();
                }
				catch (System.Net.Sockets.SocketException) {}
				catch (InvalidOperationException) {}
			}
		}

		private void ReceiveMessages()
		{
			while (stopEvent.IsSet) {
				try {
					var data = client.Receive().Datagram;
					var message = DecodeMessage(data);
					if (message is not null) {
						if (new Guid(data.Take(16).ToArray()) == Guid.Empty && message == "$serverShutdown") {
							Disconnect("Server was shut down.");
							return;
						}
						MessageReceived?.Invoke(message);
					}
				}
				catch (System.Net.Sockets.SocketException) {
					Disconnect("Connection to server was lost!");
					return;
				}
				catch (Exception e) {
					Disconnected?.Invoke(e.Message, true);
				}
			}
		}

		protected string DecodeMessage(byte[] datagram)
		{
			if (new Guid(datagram[..16]) == Id) return null;
			return CsSockets.UdpBase.Encod.GetString(datagram[16..]);
		}

		protected byte[] EncodeMessage(string message)
        {
			byte[] dataId = Id.ToByteArray();
			byte[] dataMsg = CsSockets.UdpBase.Encod.GetBytes(message);
			byte[] datagram = new byte[dataId.Length + dataMsg.Length];
			dataId.CopyTo(datagram, 0);
			dataMsg.CopyTo(datagram, dataId.Length);
			return datagram;
        }

        public void SendMessage(string message) => client.Send(EncodeMessage($"{UserName}: {message}"));

        public void Disconnect(string message)
		{
			if (message is not null)
				Disconnected?.Invoke("\r" + message, false);
			Stop();
			client.Send(EncodeMessage("$userLeft " + UserName));
		}

		public void Dispose()
		{
			Disconnect(null);
		}
	}
}
