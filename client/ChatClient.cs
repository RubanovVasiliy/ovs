using System.Net;
using System.Net.Sockets;
using System.Text;

namespace test_client
{
    internal class ChatClient
    {
        private readonly List<int> _servers;
        private int _currentServerIndex;
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isConnected;
        private readonly int _port;

        public ChatClient(int port)
        {
            _servers = new List<int>
            {
                8888,
                2222
            };
            _currentServerIndex = 0;
            _isConnected = false;
            _port = port;
        }

        private async void Connect()
        {
            var currentServer = _servers[_currentServerIndex];
            try
            {
                var ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
                var ipLocalEndPoint = new IPEndPoint(ipAddress, _port);

                _client = new TcpClient(ipLocalEndPoint);
                await _client.ConnectAsync("127.0.0.1", currentServer);
                await using var _stream = _client.GetStream();
                _isConnected = true;
                Console.WriteLine("Connected to {0}", currentServer);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Connection error: {0}", ex.Message);
                Thread.Sleep(100);
                Reconnect();
            }
        }

        private void Disconnect()
        {
            _client.Close();
            _stream = null;
            _isConnected = false;
            Console.WriteLine("Disconnected");
        }

        private void Reconnect()
        {
            _currentServerIndex++;
            _currentServerIndex %= _servers.Count;
            Disconnect();
            Connect();
        }

        public void SendMessage()
        {
            if (!_isConnected)
            {
                Connect();
            }

            while (true)
            {
                try
                {
                    Console.WriteLine("Enter your choice (rock, paper, scissors) or q for exit:");
                    var choice = Console.ReadLine();
                    if (choice is "q")
                    {
                        break;
                    }
                    if (choice is not ("rock" or "paper" or "scissors"))
                    {
                        Console.WriteLine("Invalid choice. Please try again.");
                        continue;
                    }

                    var buffer = Encoding.UTF8.GetBytes(choice);
                    _stream.Write(buffer, 0, buffer.Length);

                    buffer = new byte[1024];
                    var bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Result: {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending message: {0}", ex.Message);
                    Reconnect();
                }
            }

            Disconnect();
        }
    }
}
