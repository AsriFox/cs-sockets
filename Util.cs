using System;

namespace CsSockets
{
	public static class Util
	{
		const string settingsDefaultLocation = "settings.xml";

		public static SettingsTable ConsoleStart(string[] args)
		{
			SettingsTable settings;
			switch (args.Length) {
				case 0:
					Console.Write("Host name (leave blank to read from settings): ");
					settings = new() { Host = Console.ReadLine() };
					if (settings.Host.Length == 0) {
						settings = SettingsTable.Read(settingsDefaultLocation);
						if (settings is not null)
							return settings;
						goto user_input;
					}
					Console.Write("Port: ");
					settings.Port = int.Parse(Console.ReadLine());
					goto persist;

				case 1:
					settings = SettingsTable.Read(args[0]);
					if (settings is not null)
						return settings;
				user_input:
					Console.WriteLine("Configuration file not found.");
					Console.Write("Host name: ");
					settings.Host = Console.ReadLine();
					Console.Write("Port: ");
					settings.Port = int.Parse(Console.ReadLine());
					goto persist;

				case 2:
				case 3:
					settings = new() { Host = args[0], Port = int.Parse(args[1]) };
				persist:
					try {
						string settingsLocation = args.Length > 2 ? args[2] : settingsDefaultLocation;
						settings.Write(settingsLocation);
						Console.WriteLine($"Settings written to '{settingsLocation}'.");
					}
					catch (System.IO.IOException) {
						Console.WriteLine("Could not write settings to the configuration file.\nSettings are not persistent and will reset on the next launch.");
					}
					return settings;

				default:
					throw new ArgumentOutOfRangeException(nameof(args), $"Too many arguments ({args.Length}): expected no more than 3");
			}
		}
	}
}
