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

		UdpListener listener;
		readonly List<ClientObject> clients = new();

		// protected internal void AddConnection(ClientObject client) => clients.Add(client);

		// protected internal void RemoveConnection(string id) => clients.Remove(clients.FirstOrDefault(c => c.Id == id));

		protected internal void Listen(object parameter)
		{
			if (parameter is not int port)
				throw new ArgumentException("Parameter must be of type 'int'", nameof(parameter));
			try {
                listener = new(new IPEndPoint(IPAddress.Any, port));
				Console.WriteLine("Server is running.");

				while (true) {
					var received = listener.Receive();
					if (received.Message.Length > 10 && received.Message.StartsWith("$user")) {
						string userName = received.Message[10..];
						switch (received.Message[5..10]) {
							case "Join ":
								clients.Add(new(userName, received.Sender));
                                BroadcastMessage($"{userName} joined the chat");
                                listener.Reply($"Welcome to the server, {userName}!", received.Sender);
                                continue;

							case "Left ":
                                clients.RemoveAll(client => client.UserName == userName && client.EndPoint.Equals(received.Sender));
                                BroadcastMessage($"{userName} left the chat");
                                continue;
						}
					}
					BroadcastMessage(received.Message);
				}
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
                Environment.Exit(0);
            }
		}

		protected internal void BroadcastMessage(string message)
		{
			Console.WriteLine(message);
			foreach (var client in clients)
                listener.Reply(message, client.EndPoint);
		}
	}
}
