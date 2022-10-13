using System;
using System.Net;
using System.Threading;

namespace CsSocketServer
{
	internal class Program
	{
		static CsSockets.SettingsTable settings;
		// static ServerObject server;
		static Thread listenThread;

		static void Main(string[] args)
		{
			settings = CsSockets.Util.ConsoleStart(args);
			try {
                //listenThread = new Thread(ServerObject.Instance.Listen);
                //listenThread.Start(settings.Port);
                //listenThread = new(async (param) => {
                //	if (param is not UdpListener server)
                //		throw new InvalidCastException();
                //	while (true) {
                //		var received = await server.Receive();
                //		server.Reply("echo: " + received.Message, received.Sender);
                //		if (received.Message == "quit")
                //			break;
                //	}
                //});
                //listenThread.Start(new UdpListener(new IPEndPoint(IPAddress.Any, settings.Port)));
                // UdpListener server = new(new IPEndPoint(IPAddress.Any, settings.Port));
                listenThread = new Thread(ServerObject.Instance.Listen);
                listenThread.Start(settings.Port);
                Console.WriteLine($"Server started on port {settings.Port}");
            }
			catch (Exception e) {
				// ServerObject.Instance.Disconnect();
				Console.WriteLine(e.Message);
			}
		}
	}
}
