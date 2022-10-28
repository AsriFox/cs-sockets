namespace CsSockets
{
    using static System.Console;

    public static class Util
    {
		public readonly static System.Text.Encoding encoding = System.Text.Encoding.Unicode;

        const string settingsDefaultLocation = "settings.xml";

		public static SettingsTable ConsoleStart(string[] args)
		{
			SettingsTable settings;
			switch (args.Length) {
				case 0:
					Write("Host name (leave blank to read from settings): ");
					settings = new() { Host = ReadLine() };
					if (settings.Host.Length == 0) {
						settings = SettingsTable.Read(settingsDefaultLocation);
						if (settings is not null)
							return settings;
						goto user_input;
					}
					Write("Incoming port (read):  ");
					settings.ReadPort = int.Parse(ReadLine());
					Write("Outgoing port (write): ");
					settings.WritePort = int.Parse(ReadLine());
					goto persist;

				case 1:
					settings = SettingsTable.Read(args[0]);
					if (settings is not null)
						return settings;
				user_input:
					WriteLine("Configuration file not found.");
					Write("Host name: ");
					settings.Host = ReadLine();
                    Write("Incoming port (read):  ");
                    settings.ReadPort = int.Parse(ReadLine());
                    Write("Outgoing port (write): ");
                    settings.WritePort = int.Parse(ReadLine());
                    goto persist;

				case 2:
					throw new System.ArgumentException("Not enough arguments: expected 3 (Host name, Incoming port, Outgoing port), got 2");

				case 3:
				case 4:
					settings = new() { 
						Host = args[0], 
						ReadPort = int.Parse(args[1]), 
						WritePort = int.Parse(args[2]) 
					};
				persist:
					try {
						string settingsLocation = args.Length > 3 ? args[3] : settingsDefaultLocation;
						settings.Write(settingsLocation);
						WriteLine($"Settings written to '{settingsLocation}'.");
					}
					catch (System.IO.IOException) {
						WriteLine("Could not write settings to the configuration file.\nSettings are not persistent and will reset on the next launch.");
					}
					return settings;

				default:
					throw new System.ArgumentOutOfRangeException(nameof(args), $"Too many arguments ({args.Length}): expected no more than 4");
			}
		}
	}
}
