namespace CsSocketClient
{
	using System;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;

	internal class ClientObject : IDisposable
	{
        private readonly System.Threading.ManualResetEventSlim stopEvent = new(false);
		private readonly IPEndPoint ReceiveEP;
		//private readonly IPEndPoint SendEP;

        public event Action<string> MessageReceived;
		public event Action<string, bool> Disconnected;

        private readonly UdpClient sender;
		private readonly Guid id;
        public string UserName { get; private set; }

        public ClientObject(string host, int readPort, int writePort, string userName) 
		{
            ReceiveEP = new(IPAddress.Parse(host), readPort);
			sender = new() { ExclusiveAddressUse = false, MulticastLoopback = true };
			sender.Connect(host, writePort);
			id = Guid.NewGuid();
			UserName = userName;
		}

		public void Start()
		{
			if (IsRunning) {
				Disconnected?.Invoke("The thread is already running!", false);
				return;
            }

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
                    Start();
                }
				catch (SocketException) {}
				catch (InvalidOperationException) {}
			}
		}

		private void ReceiveMessages()
		{
			UdpClient receiver = new() { ExclusiveAddressUse = false, MulticastLoopback = true };
			receiver.JoinMulticastGroup(ReceiveEP.Address, 32);
            IPEndPoint remoteIp = null;
            while (IsRunning) {
				try {
					byte[] data = receiver.Receive(ref remoteIp);
					if (new Guid(data[..16]) != id)
						MessageReceived?.Invoke(CsSockets.UdpBase.Encod.GetString(data[16..]));
				}
				catch (SocketException) {
					Disconnect("Connection to server was lost!");
					return;
				}
				catch (Exception e) {
					receiver.Close();
					Disconnected?.Invoke(e.Message, true);
				}
			}
			receiver.Close();
		}

		public void SendMessage(string message)
		{
			byte[] data = CsSockets.UdpBase.Encod.GetBytes($"{UserName}: {message}");
			sender.Send(
				id.ToByteArray().Concat(data).ToArray(), 
				data.Length + 16
			);
        }

        public void Disconnect(string message)
		{
			if (message is not null)
				Disconnected?.Invoke("\r" + message, false);
			Stop();
		}

		public void Dispose()
		{
			Disconnect(null);
		}
	}
}
