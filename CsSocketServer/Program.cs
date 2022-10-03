using System;
using System.Threading;

namespace CsSocketServer
{
	internal class Program
	{
		const string settingsLocation = "settings.xml";
		static CsSockets.SettingsTable settings;
		// static ServerObject server;
		static Thread listenThread;

		static void Main(string[] args)
		{
			settings = CsSockets.Util.ConsoleStart(args, settingsLocation);
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
