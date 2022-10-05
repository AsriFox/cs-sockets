using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsSocketClient
{
	internal class ClientObject : IDisposable
	{
		public static readonly Encoding Encod = Encoding.Unicode;

		private ClientObject() {}
		private static ClientObject instance;
		public static ClientObject Instance => instance ??= new();

		TcpClient client;
		NetworkStream stream;

		readonly ManualResetEventSlim stopEvent = new(false);

		public event Action<string> MessageReceived;
		public event Action<string, bool> Disconnected;

		private string _host;
		private int _port;
		public void Connect(string host, int port) 
		{
			_host = host;
			_port = port;
			client = new();
			client.Connect(host, port);
			stream = client.GetStream();
		}

		private string userName;
		public string UserName {
			get => userName;
			set {
				userName = value;
				byte[] data = Encod.GetBytes(userName);
				stream.Write(data, 0, data.Length);
			}
		}

		public void Start()
		{
			if (stopEvent.IsSet) {
				Disconnected?.Invoke("The thread is already running!", false);
				return;
			}
			Thread receiveThread = new(ReceiveMessages);
			stopEvent.Set();
			receiveThread.Start();
		}

		public void Stop() => stopEvent.Reset();

		public void WaitForConnection() {
			Thread waitThread = new(TryReconnecting);
			waitThread.Start();
			stopEvent.Wait();
		}

		private void TryReconnecting()
		{
			while (!stopEvent.IsSet) {
				Thread.Sleep(100);
				try {
					Connect(_host, _port);
					Start();
					SendMessage(userName);
				}
				catch (SocketException) {}
				catch (InvalidOperationException) {}
			}
		}

		private void ReceiveMessages()
		{
			while (stopEvent.IsSet) {
				try {
					var data = new byte[64];
					StringBuilder builder = new("\r");
					do {
						var bytes = stream.Read(data, 0, data.Length);
						builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
					}
					while (stream.DataAvailable && stopEvent.IsSet);
					MessageReceived?.Invoke(builder.ToString());
				}
				catch (System.IO.IOException e) {
					if (e.InnerException is not SocketException)
						throw new Exception(e.Message);
					Disconnected?.Invoke("\rConnection to server was lost!", false);
					Disconnect();
					return;
				}
				catch (Exception e) {
					Disconnected?.Invoke(e.Message, true);
					// Disconnect();
				}
			}
		}

		public void SendMessage(string message)
		{
			byte[] data = Encoding.Unicode.GetBytes(message);
			stream.Write(data, 0, data.Length);
		}

		public void Disconnect()
		{
			Stop();
			stream?.Close();
			client?.Close();
		}

		public void Dispose()
		{
			Disconnect();
		}
	}
}
