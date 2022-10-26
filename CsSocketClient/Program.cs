namespace CsSocketClient
{
    using static System.Console;

    internal class Program
	{
		static CsSockets.SettingsTable settings;
        static ClientObject client;
		static bool locked = false;

		static void Main(string[] args)
		{
			settings = CsSockets.Util.ConsoleStart(args);
			try {
                Write("Your name: ");
                client = new(settings.Host, settings.Port, ReadLine());
                WriteLine($"Communicating with server at {settings.Host}:{settings.Port}");

                client.MessageReceived += OnMessageReceived;
                client.Disconnected += OnDisconnected;
                client.Start();

                CancelKeyPress += (_, _) => System.Environment.Exit(0);
                System.AppDomain.CurrentDomain.ProcessExit += (_, _) => client.Dispose();

                while (true) {
					string message = ReadLine() ?? throw new System.OperationCanceledException("Cancelled");
					if (locked) {
                        WriteLine("Waiting for connection...");
                        client.WaitForConnection();
                        WriteLine($"\rConnection restored. Welcome back, {client.UserName}!");
                        locked = false;
                    }
                    else client.SendMessage(message);
                    Write("> ");
                }
			}
            // catch (System.OperationCanceledException) {}
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
