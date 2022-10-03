using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CsSocketServer
{
	internal class ServerObject
	{
		private ServerObject() {}
		static ServerObject instance;
		public static ServerObject Instance => instance ??= new();

		TcpListener tcpListener;
		readonly List<ClientObject> clients = new();

		protected internal void AddConnection(ClientObject client) => clients.Add(client);

		protected internal void RemoveConnection(string id) => clients.Remove(clients.FirstOrDefault(c => c.Id == id));

		protected internal void Listen(object parameter)
		{
			if (parameter is not int port)
				throw new ArgumentException("Parameter must be of type 'int'", nameof(parameter));
			try {
				tcpListener = new(IPAddress.Any, port);
				tcpListener.Start();
				Console.WriteLine("Server is running.");

				while (true) {
					var tcpClient = tcpListener.AcceptTcpClient();
					ClientObject client = new(tcpClient, this);
					Thread clientThread = new(new ThreadStart(client.Process));
					clientThread.Start();
				}
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
				Disconnect();
			}
		}

		protected internal void BroadcastMessage(string message, string id)
		{
			var data = Encoding.Unicode.GetBytes(message);
			foreach (var client in clients)
				if (client.Id != id)
					client.Stream.Write(data, 0, data.Length);
		}

		protected internal void Disconnect()
		{
			tcpListener.Stop();
			foreach (var client in clients)
				client.Close();
			Environment.Exit(0);
		}
	}
}
