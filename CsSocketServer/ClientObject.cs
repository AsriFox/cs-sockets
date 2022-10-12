using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CsSocketServer
{
	internal class ClientObject
	{
		protected internal string Id { get; private set; }
		protected internal NetworkStream Stream { get; private set; }
		string userName;
		TcpClient client;
		ServerObject server;

		public ClientObject(TcpClient tcpClient, ServerObject serverObject)
		{
			Id = Guid.NewGuid().ToString();
			client = tcpClient;
			server = serverObject;
			serverObject.AddConnection(this);
		}

		public void Process()
		{
			try {
				Stream = client.GetStream();
				string message = GetMessage();
				userName = message;
				message = $"{userName} joined the chat";
				server.BroadcastMessage(message, this.Id);
				Console.WriteLine(message);
				while (true) {
					try {
						message = $"{userName}: {GetMessage()}";
						Console.WriteLine(message);
						server.BroadcastMessage(message, this.Id);
					}
					catch {
						message = $"{userName} left the chat";
						Console.WriteLine(message);
						server.BroadcastMessage(message, this.Id);
						break;
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
			}
			finally {
				server.RemoveConnection(this.Id);
				Close();
			}
		}

		private string GetMessage()
		{
			var data = new byte[64];
			StringBuilder builder = new();
			do {
				var bytes = Stream.Read(data, 0, data.Length);
				if (bytes == 0)
					throw new SocketException();
				builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
			}
			while (Stream.DataAvailable);
			return builder.ToString();
		}

		protected internal void Close()
		{
			Stream?.Close();
			client?.Close();
		}
	}
}
