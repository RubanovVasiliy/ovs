using System.Net.Sockets;
using System.Text;

namespace client;

internal class ChatClient
{
    private readonly List<string> _servers;
    private int _currentServerIndex;
    private TcpClient _client;
    private NetworkStream _stream;
    private bool _isConnected;

    public ChatClient()
    {
        _servers = new List<string>
        {
            "0.0.0.0",
            "server2.com",
            "server3.com"
        };
        _currentServerIndex = 0;
        _isConnected = false;
    }

    private void Connect()
    {
        string currentServer = _servers[_currentServerIndex];
        try
        {
            _client = new TcpClient(currentServer, 8888);
            _stream = _client.GetStream();
            _isConnected = true;
            Console.WriteLine("Connected to {0}", currentServer);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Connection error: {0}", ex.Message);
            Reconnect();
        }
    }

    private void Disconnect()
    {
        if (_client != null)
        {
            _client.Close();
            _stream = null;
            _isConnected = false;
            Console.WriteLine("Disconnected");
        }
    }

    private void Reconnect()
    {
        _currentServerIndex++;
        if (_currentServerIndex >= _servers.Count)
        {
            _currentServerIndex = 0;
        }

        Disconnect();
        Connect();
    }

    public void SendMessage(string message)
    {
        if (!_isConnected)
        {
            Connect();
        }

        byte[] data = Encoding.UTF8.GetBytes(message);
        try
        {
            _stream.Write(data, 0, data.Length);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Error sending message: {0}", ex.Message);
            Reconnect();
        }
    }
}