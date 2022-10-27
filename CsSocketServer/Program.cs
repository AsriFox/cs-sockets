namespace CsSocketServer
{
	using System.Net;
	using System.Net.Sockets;
	using static System.Console;

    internal class Program
	{
		static CsSockets.SettingsTable settings;
		static System.Threading.Thread listenThread;
		// static ServerObject server;

		static void Main(string[] args)
		{
			WriteLine("This server uses the hostname to broadcast messages.\nPlease, enter a broadcast address for the subnet (e.g. 192.168.0.255).");
			settings = CsSockets.Util.ConsoleStart(args);
            try {
				//CancelKeyPress += (_, _) => System.Environment.Exit(0);
				//System.AppDomain.CurrentDomain.ProcessExit += (_, _) => ServerObject.Instance.Dispose();
				//server = new(settings.Host, settings.ReadPort);

				//listenThread = new(ServerObject.Instance.Listen);
				//listenThread.Start(settings.Port);
				listenThread = new(Listen);
				listenThread.Start();
                WriteLine($"Server is open at {settings.Host}:{settings.ReadPort}");
				//server.Write += OnWrite;
            }
			catch (System.Exception e) {
				WriteLine(e.Message);
			}
		}

		// static void OnWrite(string message) => WriteLine(message);

		static void Listen()
		{
			UdpClient listener = new() { ExclusiveAddressUse = false, MulticastLoopback = true };
			UdpClient sender = new(settings.Host, settings.ReadPort);
			IPEndPoint listenEp = null;
			while (true) {
				byte[] data = listener.Receive(ref listenEp);
				WriteLine(CsSockets.UdpBase.Encod.GetString(data[16..]));
				sender.Send(data, data.Length);
			}
		}
	}
}
