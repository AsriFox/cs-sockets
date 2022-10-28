namespace CsSocketClient
{
    using System;
    using CsSockets;
	using static System.Console;

    internal class Program
	{
		static string GetLine() => ReadLine() ?? throw new OperationCanceledException("Cancelled");

		static SettingsTable settings;
		static string userName;

		static void Main(string[] args)
		{
			try {
				settings = Util.ConsoleStart(args);

				Write("Your name: ");
				userName = GetLine();

				System.Threading.Thread receiveThread = new(ReceiveMessages);
                receiveThread.Start();
                SendMessages();
            }
            catch (Exception ex) {
                WriteLine(ex.Message);
            }
        }

		static void SendMessages()
        {
            UdpSender sender = new(settings.Host, settings.WritePort);
            while (true) {
                Write("> ");
                sender.Send($"{userName}: {GetLine()}");
            }
        }

        static void ReceiveMessages()
        {
            UdpReceiver receiver = new(settings.Host, settings.ReadPort);
            var remoteAddress = System.Net.IPAddress.Parse(settings.Host);
            System.Net.IPEndPoint? remoteEp = null;
            while (true) {
                byte[] data = receiver.Receive(ref remoteEp);
                if (!remoteEp.Address.Equals(remoteAddress))
                    continue;   // Receive only messages from the server;
                WriteLine($"\r{Util.encoding.GetString(data)}> ");
            }
        }
    }
}
