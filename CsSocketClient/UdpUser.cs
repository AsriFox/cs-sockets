namespace CsSocketClient
{
    internal class UdpUser : CsSockets.UdpBase
    {
        private UdpUser() {}

        public static UdpUser ConnectTo(string hostName, int portSend, int portReceive)
        {
            UdpUser connection = new();
            connection.Client.Connect(hostName, portSend);
            return connection;
        }

        // public void Send(string message)
        public void Send(byte[] message)
        {
            // var datagram = Encod.GetBytes(message);
            Client.Send(message, message.Length);
        }
    }
}
