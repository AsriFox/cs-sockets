using System;

namespace CsSockets
{
	public static class Util
	{
		public static SettingsTable ConsoleStart(string[] args, string settingsLocation)
		{
			var settings = SettingsTable.Read(settingsLocation);
			if (settings is null) {
				string host; int port;
				if (args.Length == 2) {
					host = args[0];
					port = int.Parse(args[1]);
				}
				else {
					Console.WriteLine("Configuration file not found.");
					Console.Write("Host name: ");
					host = Console.ReadLine();
					Console.Write("Port: ");
					port = int.Parse(Console.ReadLine());
				}
				settings = new SettingsTable { Host = host, Port = port };
				settings.Write(settingsLocation);
				Console.WriteLine($"Settings written to '{settingsLocation}'.");
			}
			return settings;
		}
	}
}
