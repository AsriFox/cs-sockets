namespace CsSocketClient
{
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using static System.Console;

    internal class Program
	{
		static CsSockets.SettingsTable settings;
        //static ClientObject client;
        //static bool locked = false;
        static string userName;

		static void Main(string[] args)
		{
            WriteLine("For multicast: use addresses from 224.0.0.0 to 239.255.255.255");
			settings = CsSockets.Util.ConsoleStart(args);

            Write("User name: ");
            userName = ReadLine();

            System.Threading.Thread receiveThread = new(ReceiveMessages);
            receiveThread.Start();

            SendMessages();

			//try {
   //             Write("Your name: ");
   //             client = new(settings.Host, settings.Port, ReadLine());
   //             WriteLine($"Communicating with server at {settings.Host}:{settings.Port}");

   //             CancelKeyPress += OnCancelKeyPress;
   //             client.MessageReceived += OnMessageReceived;
   //             client.Disconnected += OnDisconnected;
   //             client.Start();

   //             AppDomain.CurrentDomain.ProcessExit += (_, _) => client.Dispose();

   //             while (true) {
			//		string message = ReadLine() ?? throw new OperationCanceledException("\rClient is closing now.");
			//		if (locked) {
   //                     WriteLine("Waiting for connection...");
   //                     client.WaitForConnection();
   //                     WriteLine($"\rConnection restored. Welcome back, {client.UserName}!");
   //                     locked = false;
   //                 }
   //                 else client.SendMessage(message);
   //                 Write("> ");
   //             }
			//}
			//catch (Exception e) {
			//	WriteLine(e.Message);
			//}
		}

        private static void SendMessages()
        {
            UdpClient sender = new() { ExclusiveAddressUse = false, MulticastLoopback = true };
            IPEndPoint endPoint = new(IPAddress.Parse(settings.Host), settings.WritePort);
            try
            {
                while (true)
                {
                    byte[] data = CsSockets.UdpBase.Encod.GetBytes($"{userName}: {ReadLine()}");
                    sender.Send(data, data.Length, endPoint);
                }
            }
            catch (System.Exception ex)
            {
                WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }

        private static void ReceiveMessages()
        {
            //if (parameter is not int localPort) 
            //    throw new System.InvalidCastException();

            UdpClient receiver = new() { ExclusiveAddressUse = false, MulticastLoopback = true };
            receiver.Connect(settings.Host, settings.ReadPort);
            receiver.JoinMulticastGroup(IPAddress.Parse(settings.Host), 50);
            IPEndPoint remoteIp = null;
            var localAddress = LocalIPAddress();
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp);
                    //if (remoteIp.Address.Equals(localAddress))
                    //    continue;
                    string message = CsSockets.UdpBase.Encod.GetString(data);
                    WriteLine(message);
                }
            }
            catch (System.Exception ex)
            {
                WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }

        private static IPAddress LocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip;
            return null;
        }

        //static void OnMessageReceived(string message) => Write("\r" + message + "\n> ");

        //static void OnDisconnected(string message, bool shutdown)
        //{
        //    WriteLine(message);
        //    if (shutdown) Environment.Exit(0);
        //    locked = true;
        //    WriteLine("Close the terminal or press 'Ctrl+C' to exit.\nPress 'Enter' to wait for connection.");
        //}

        //static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        //{
        //    if (client is not null && client.IsRunning && !locked)
        //    {
        //        WriteLine("\rExit? [y/n]: ");
        //        switch (ReadKey().Key) 
        //        {
        //            case ConsoleKey.Y:
        //                client.SendMessage($"\r{client.UserName} has left the server.");
        //                break;
        //            case ConsoleKey.N:
        //                Write("\r> ");
        //                return;
        //            default:
        //                Write("\nUnknown option. Resuming in 1s.");
        //                System.Threading.Thread.Sleep(1000);
        //                Write("\r> ");
        //                return;
        //        }
        //    }
        //    Environment.Exit(0);
        //}
    }
}
