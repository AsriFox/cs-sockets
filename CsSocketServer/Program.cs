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
				CancelKeyPress += (_, _) => System.Environment.Exit(0);
				System.AppDomain.CurrentDomain.ProcessExit += (_, _) => ServerObject.Instance.Dispose();

                listenThread = new(ServerObject.Instance.Listen);
                listenThread.Start(settings.Port);
                WriteLine($"Server started on port {settings.Port}");
				ServerObject.Instance.Write += OnWrite;
            }
			catch (System.Exception e) {
				WriteLine(e.Message);
			}
		}

		static void OnWrite(string message) => WriteLine(message);
	}
}
