namespace CsSocketClient
{
	using System;

	internal class ClientObject : IDisposable
	{
		private ClientObject() {}
		private static ClientObject instance;
		public static ClientObject Instance => instance ??= new();

		UdpUser client;

		readonly System.Threading.ManualResetEventSlim stopEvent = new(false);

		public event Action<string> MessageReceived;
		public event Action<string, bool> Disconnected;

		private string _host;
		private int _port;
		public void Connect(string host, int port) 
		{
			_host = host;
			_port = port;
			client = UdpUser.ConnectTo(host, port);
		}

		private string userName = null;
		public string UserName {
			get => userName;
			set {
				if (userName is not null)
					throw new InvalidOperationException("User name cannot be changed while connected!");
				userName = value;
                client.Send("$userJoin " + userName);
            }
		}

		public void Start()
		{
			if (stopEvent.IsSet) {
				Disconnected?.Invoke("The thread is already running!", false);
				return;
			}
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
                System.Threading.Thread.Sleep(100);
				try {
					Connect(_host, _port);
					Start();
					SendMessage(userName);
				}
				catch (System.Net.Sockets.SocketException) {}
				catch (InvalidOperationException) {}
			}
		}

		private void ReceiveMessages()
		{
			while (stopEvent.IsSet) {
				try {
                    var received = client.Receive();
                    if (!received.Message.StartsWith(userName))
                        MessageReceived?.Invoke(received.Message);
				}
				catch (System.Net.Sockets.SocketException) {
					Disconnected?.Invoke("\rConnection to server was lost!", false);
					Disconnect();
					return;
				}
				catch (Exception e) {
					Disconnected?.Invoke(e.Message, true);
				}
			}
		}

		public void SendMessage(string message) => client.Send($"{userName}: {message}");

		public void Disconnect()
		{
			Stop();
			client.Send("$userLeft " + userName);
		}

		public void Dispose()
		{
			Disconnect();
		}
	}
}
