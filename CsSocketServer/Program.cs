namespace CsSocketServer
{
    using static System.Console;

    internal class Program
	{
		static CsSockets.SettingsTable settings;
		static System.Threading.Thread listenThread;

		static void Main(string[] args)
		{
			settings = CsSockets.Util.ConsoleStart(args);
			try {
                listenThread = new(ServerObject.Instance.Listen);
                listenThread.Start(settings.Port);
                WriteLine($"Server started on port {settings.Port}");
            }
			catch (System.Exception e) {
				WriteLine(e.Message);
			}
		}
	}
}
