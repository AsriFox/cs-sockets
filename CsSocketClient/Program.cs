using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CsSocketClient
{
	internal class Program
	{
		const string settingsLocation = "settings.xml";
		static CsSockets.SettingsTable settings;
		static bool locked = false;

		static void Main(string[] args)
		{
			settings = CsSockets.Util.ConsoleStart(args, settingsLocation);
			try {
				ClientObject.Instance.Connect(settings.Host, settings.Port);
				Console.WriteLine($"Established connection with server at {settings.Host}:{settings.Port}");
				
				Console.Write("Your name: ");
				ClientObject.Instance.UserName = Console.ReadLine();

				ClientObject.Instance.MessageReceived += OnMessageReceived;
				ClientObject.Instance.Disconnected += OnDisconnected;

				ClientObject.Instance.Start();
				Console.WriteLine($"Welcome, {ClientObject.Instance.UserName}!");
				
				while (true) {
					Console.Write("> ");
					string message = Console.ReadLine();
					if (locked) {
						Console.WriteLine("Waiting for connection...");
						ClientObject.Instance.WaitForConnection();
					}
					ClientObject.Instance.SendMessage(message);
				}
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
				// ClientObject.Instance.Disconnect();
			}
		}


		static void OnMessageReceived(string message)
		{
			Console.WriteLine(message);
			Console.Write("> ");
		}

		static void OnDisconnected(string message, bool shutdown)
		{
			Console.WriteLine(message);
			if (shutdown) Environment.Exit(0);
			locked = true;
			Console.WriteLine("Close the terminal or press 'Ctrl+C' to exit.");
			Console.WriteLine("Press 'Enter' to wait for connection.");
		}
	}
}
