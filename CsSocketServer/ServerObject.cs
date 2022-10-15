namespace CsSocketServer
{
    using System.Net;

    internal struct ClientObject
    {
        public string UserName { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        public ClientObject(string userName, IPEndPoint endPoint)
        {
            UserName = userName;
            EndPoint = endPoint;
        }
    }

    internal class ServerObject
	{
		private ServerObject() {}
		static ServerObject instance;
		public static ServerObject Instance => instance ??= new();

        public event System.Action<string> Write;

        UdpListener listener;
		readonly System.Collections.Generic.List<ClientObject> clients = new();

		protected internal void Listen(object parameter)
		{
			if (parameter is not int port)
				throw new System.ArgumentException("Parameter must be of type 'int'", nameof(parameter));
			try {
                listener = new(new IPEndPoint(IPAddress.Any, port));
				Write?.Invoke("Server is running.");

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
			catch (System.Exception e) {
                Write?.Invoke(e.Message);
                System.Environment.Exit(0);
            }
		}

		protected internal void BroadcastMessage(string message)
		{
            Write?.Invoke(message);
			foreach (var client in clients)
                listener.Reply(message, client.EndPoint);
		}
	}
}
