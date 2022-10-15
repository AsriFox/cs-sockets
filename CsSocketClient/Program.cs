namespace CsSocketClient
{
    using static System.Console;

    internal class Program
	{
		static CsSockets.SettingsTable settings;
		static bool locked = false;

		static void Main(string[] args)
		{
			settings = CsSockets.Util.ConsoleStart(args);
			try {
				ClientObject.Instance.Connect(settings.Host, settings.Port);
                WriteLine($"Communicating with server at {settings.Host}:{settings.Port}");

                Write("Your name: ");
				ClientObject.Instance.UserName = ReadLine();

				ClientObject.Instance.MessageReceived += OnMessageReceived;
				ClientObject.Instance.Disconnected += OnDisconnected;

				ClientObject.Instance.Start();
                CancelKeyPress += (_, _) => System.Environment.Exit(0);
                System.AppDomain.CurrentDomain.ProcessExit += (_, _) => ClientObject.Instance.Dispose();

                while (true) {
					Write("> ");
					string message = ReadLine() ?? throw new System.OperationCanceledException("Cancelled");
					if (locked) {
                        WriteLine("Waiting for connection...");
                        ClientObject.Instance.WaitForConnection();
                        WriteLine($"Connection restored. Welcome back, {ClientObject.Instance.UserName}!");
                        locked = false;
                    }
                    ClientObject.Instance.SendMessage(message);
                }
			}
            catch (System.OperationCanceledException) {}
			catch (System.Exception e) {
				WriteLine(e.Message);
			}
		}

        static void OnMessageReceived(string message) => Write("\r" + message + "\n> ");

        static void OnDisconnected(string message, bool shutdown)
        {
            WriteLine(message);
            if (shutdown) System.Environment.Exit(0);
            locked = true;
            WriteLine("Close the terminal or press 'Ctrl+C' to exit.");
            WriteLine("Press 'Enter' to wait for connection.");
        }
    }
}
