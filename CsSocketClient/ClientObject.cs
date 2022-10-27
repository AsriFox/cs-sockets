namespace CsSocketClient
{
	using CsSockets;
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
        public string UserName { get; private set; }

        public ClientObject(string host, int port, string userName) 
		{
			_host = host;
			_port = port;
			client = UdpUser.ConnectTo(host, port, port);

			UserName = userName;
		}

		public void Start()
		{
			if (IsRunning) {
				Disconnected?.Invoke("The thread is already running!", false);
				return;
			}

			// Greet the server:
			SendMessage($"\r{UserName} has joined the server.");

			// Start receiving messages:
            System.Threading.Thread receiveThread = new(ReceiveMessages);
			stopEvent.Set();
			receiveThread.Start();
		}

		public void Stop() => stopEvent.Reset();

		public bool IsRunning => stopEvent.IsSet;

		public void WaitForConnection() {
            System.Threading.Thread waitThread = new(TryReconnecting);
			waitThread.Start();
			stopEvent.Wait();
		}

		private void TryReconnecting()
		{
			while (!IsRunning) {
                System.Threading.Thread.Sleep(1000);
				try {
					client = UdpUser.ConnectTo(_host, _port, _port);
                    Start();
                }
				catch (System.Net.Sockets.SocketException) {}
				catch (InvalidOperationException) {}
			}
		}

		private Guid lastMessageId = Guid.Empty;

		private void ReceiveMessages()
		{
			while (IsRunning) {
				try {
					var data = client.Receive().Datagram;
					if (new Guid(data[..16]) != lastMessageId)
						MessageReceived?.Invoke(UdpBase.Encod.GetString(data[16..]));

					//var message = DecodeMessage(data);
					//if (message is not null) {
					//	if (new Guid(data.Take(16).ToArray()) == Guid.Empty && message == "$serverShutdown") {
					//		Disconnect("Server was shut down.");
					//		return;
					//	}
					//	MessageReceived?.Invoke(message);
					//}
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

		public void SendMessage(string message)
		{
			Guid id = Guid.NewGuid();
			byte[] data = UdpBase.Encod.GetBytes($"{UserName}: {message}");
			byte[] datagram = new byte[16 + data.Length];
			id.ToByteArray().CopyTo(datagram, 0);
			data.CopyTo(datagram, 16);
			client.Send(datagram);
			lastMessageId = id;
		}

        public void Disconnect(string message)
		{
			if (message is not null)
				Disconnected?.Invoke("\r" + message, false);
			Stop();
			//client.Send(EncodeMessage("$userLeft " + UserName));
		}

		public void Dispose()
		{
			Disconnect(null);
		}
	}
}
