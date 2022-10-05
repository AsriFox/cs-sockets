using System;
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
				listenThread = new Thread(ServerObject.Instance.Listen);
				listenThread.Start(settings.Port);
				Console.WriteLine($"Server started on port {settings.Port}");
			}
			catch (Exception e) {
				ServerObject.Instance.Disconnect();
				Console.WriteLine(e.Message);
			}
		}
	}
}
