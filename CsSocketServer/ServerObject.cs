namespace CsSocketServer
{
	using System;
	using System.Linq;
	using System.Net;

	internal struct ClientObject
    {
		public Guid Id { get; private set; }
        public string UserName { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        public ClientObject(string userName, IPEndPoint endPoint)
        {
			Id = Guid.NewGuid();
            UserName = userName;
            EndPoint = endPoint;
        }
    }

    internal class ServerObject : IDisposable
	{
		private ServerObject() {}
		static ServerObject instance;
		public static ServerObject Instance => instance ??= new();

        public event Action<string> Write;

        UdpListener listener;
		readonly System.Collections.Generic.List<ClientObject> clients = new();
		
		static byte[] EncodeMessage(Guid id, string mess)
        {
            byte[] dataId = id.ToByteArray();
            byte[] dataMsg = CsSockets.UdpBase.Encod.GetBytes(mess);
            byte[] datagram = new byte[dataId.Length + dataMsg.Length];
            dataId.CopyTo(datagram, 0);
            dataMsg.CopyTo(datagram, dataId.Length);
            return datagram;
        }

        protected internal void Listen(object parameter)
		{
			if (parameter is not int port)
				throw new ArgumentException("Parameter must be of type 'int'", nameof(parameter));
			try {
                listener = new(new IPEndPoint(IPAddress.Any, port));
				Write?.Invoke("Server is running.");

				while (true) {
					var received = listener.Receive();
					Guid idr = new(received.Datagram[..16]);
                    string message = CsSockets.UdpBase.Encod.GetString(received.Datagram[16..]);
					try {
						var client = clients.First(c => c.Id == idr);

						// User with this id exists
						if (message.StartsWith("$userLeft")) {
							if (clients.Remove(client)) {
                                message = $"{client.UserName} left the chat";
                                Write?.Invoke(message);
                                BroadcastMessage(EncodeMessage(client.Id, message));
                            }
                        }
						else {
                            Write?.Invoke(message);
                            BroadcastMessage(received.Datagram);
                        }
					}
					catch (InvalidOperationException) {
						// No user with this id exists
						if (!message.StartsWith("$userJoin ")) continue;

                        // Try adding
						ClientObject client = new(message[10..], received.Sender);
						clients.Add(client);

                        listener.Reply(
                            EncodeMessage(
                                client.Id,
                                $"Welcome to the server, {client.UserName}!"),
                            client.EndPoint);
                        // System.Threading.Thread.Sleep(100);

                        message = $"{client.UserName} joined the chat";
                        Write?.Invoke(message);
                        BroadcastMessage(EncodeMessage(client.Id, message));
                    }
				}
			}
			catch (Exception e) {
                Write?.Invoke(e.Message);
                Environment.Exit(0);
            }
		}

		protected internal void BroadcastMessage(byte[] message)
		{
			foreach (var client in clients)
                listener.Reply(message, client.EndPoint);
		}

        public void Dispose()
        {
            if (listener is null) return;
            // Notify users about the server shutdown:
            BroadcastMessage(EncodeMessage(Guid.Empty, "$serverShutdown"));
        }
	}
}
