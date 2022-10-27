namespace CsSocketClient
{
    using System;
    using static System.Console;

    internal class Program
	{
		static CsSockets.SettingsTable settings;
        static ClientObject client;

		static void Main(string[] args)
		{
            WriteLine("For multicast: use addresses from 224.0.0.0 to 239.255.255.255");
			settings = CsSockets.Util.ConsoleStart(args);

            Write("Your name: ");
            client = new(settings.Host, settings.ReadPort, settings.WritePort, ReadLine());

            CancelKeyPress += OnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += (_, _) => client.Dispose();
            client.MessageReceived += OnMessageReceived;
            client.Disconnected += OnDisconnected;

            client.Start();
            try {
                while (true) {
                    client.SendMessage($"{client.UserName}: {ReadLine()}");
                }
            }
            catch (Exception ex) {
                WriteLine(ex.Message);
                Environment.Exit(0);
            }
		}

        static void OnMessageReceived(string message) => Write("\r" + message + "\n> ");

        static void OnDisconnected(string message, bool shutdown)
        {
            WriteLine(message);
            if (shutdown) Environment.Exit(0);
            // locked = true;
            WriteLine("Close the terminal or press 'Ctrl+C' to exit.\nPress 'Enter' to wait for connection.");
        }

        static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (client is not null && client.IsRunning) // && !locked)
            {
                WriteLine("\rExit? [y/n]: ");
                switch (ReadKey().Key) 
                {
                    case ConsoleKey.Y:
                        client.SendMessage($"\r{client.UserName} has left the server.");
                        break;
                    case ConsoleKey.N:
                        Write("\r> ");
                        return;
                    default:
                        Write("\nUnknown option. Resuming in 1s.");
                        System.Threading.Thread.Sleep(1000);
                        Write("\r> ");
                        return;
                }
            }
            Environment.Exit(0);
        }
    }
}
