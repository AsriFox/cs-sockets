namespace CsSocketServer
{
	using System;
	using CsSockets;
	using static System.Console;

    internal class Program
	{
		static SettingsTable settings;

		static void Main(string[] args)
		{
			WriteLine("This server uses the hostname to broadcast messages.\nPlease, enter a broadcast address for the subnet (e.g. 192.168.0.255).");
			settings = Util.ConsoleStart(args);

			UdpSender sender = new(settings.Host, settings.ReadPort);
			UdpReceiver receiver = new(settings.WritePort);

			// Send echo for calibration:
            System.Net.IPEndPoint? remoteEp = null;
			var id = Guid.NewGuid();
			sender.Send(id.ToByteArray()).Wait();
			byte[] data = receiver.Receive(ref remoteEp);
			if (new Guid(data) != id)
				throw new OperationCanceledException("Unable to retrieve local IP endpoint");

            // Retrieve local IP endpoint:
            System.Net.IPEndPoint localEp = remoteEp;
            WriteLine($"Server is open at {settings.Host}:{settings.ReadPort}");

            try {
                while (true) {
					data = receiver.Receive(ref remoteEp);

					// Discard loopback messages:
					if (remoteEp.Equals(localEp)) continue;

					string message = Util.encoding.GetString(data);
					WriteLine(message);

					// Rebroadcast messages:
					sender.Send(data);
				}
            }
			catch (Exception e) {
				WriteLine(e.Message);
			}
		}
	}
}
