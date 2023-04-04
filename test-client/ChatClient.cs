using System.Net;
using System.Net.Sockets;
using System.Text;

namespace test_client;

class ChatClient
{
    private readonly List<int> _servers;
    private int _currentServerIndex;
    private TcpClient _client;
    private NetworkStream? _stream;
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

    private void Connect()
    {
        var currentServer = _servers[_currentServerIndex];
        try
        {
            var ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
            var ipLocalEndPoint = new IPEndPoint(ipAddress, _port);

            _client = new TcpClient(ipLocalEndPoint);
            _client.Connect("127.0.0.1", currentServer);
            _isConnected = true;
            //Console.WriteLine("Connected to {0}", currentServer);
        }
        catch (SocketException ex)
        {
            //Console.WriteLine("Connection error: {0}", ex.Message);
            Thread.Sleep(100);
            Reconnect();
        }
    }

    private void Disconnect()
    {
        _client.Close();
        _stream = null;
        _isConnected = false;
        //Console.WriteLine("Disconnected");
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

        string? choice = null;


        while (true)
        {
            try
            {
                do
                {
                    Console.WriteLine("Enter your choice (rock, paper, scissors) or q for exit:");
                    choice = Console.ReadLine();
                } while (choice is not "q" && choice is not "rock" && choice is not "paper" &&
                         choice is not "scissors");

                if (_client.Client is { Connected: false }) throw new Exception("client is not connected");

                _stream = _client.GetStream();
                if (!_stream.CanWrite) throw new Exception("can not get steram");

                var buffer = Encoding.UTF8.GetBytes(choice ?? " ");
                _stream.Write(buffer, 0, buffer.Length);

                if (choice is "q") break;
                buffer = new byte[1024];

                if (!_stream.CanRead) continue;
                var bytesRead = _stream.Read(buffer, 0, buffer.Length);
                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                _stream.Close();
                Console.WriteLine($"Result: {message}");
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error sending message: {0}", ex.Message);
                Reconnect();

                _stream = _client.GetStream();
                var buffer = Encoding.UTF8.GetBytes(choice ?? " ");
                _stream.Write(buffer, 0, buffer.Length);

                if (choice is "q") break;
                buffer = new byte[1024];
                var bytesRead = _stream.Read(buffer, 0, buffer.Length);
                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Result: {message}");
            }
        }
        //Disconnect();
    }
}
