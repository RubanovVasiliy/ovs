using System.Net;
using System.Net.Sockets;
using System.Text;

namespace server;

internal class ChatServer
{
    private readonly TcpListener _listener;
    private bool _isRunning;

    public ChatServer()
    {
        var ip = IPAddress.Any;
        _listener = new TcpListener(ip, 8888);
        _isRunning = false;
        Console.WriteLine(ip);
    }

    public void Start()
    {
        _isRunning = true;
        _listener.Start();
        Console.WriteLine("Server started");

        while (_isRunning)
        {
            try
            {
                var client = _listener.AcceptTcpClient();
                Console.WriteLine("Client connected");

                // Start new thread for client communication
                var clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error accepting client: {0}", ex.Message);
            }
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _listener.Stop();
        Console.WriteLine("Server stopped");
    }

    private void HandleClient(TcpClient client)
    {
        var stream = client.GetStream();
        var data = new byte[1024];

        while (true)
        {
            int bytesRead;

            try
            {
                bytesRead = stream.Read(data, 0, data.Length);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error reading message: {0}", ex.Message);
                break;
            }

            if (bytesRead == 0)
            {
                Console.WriteLine("Client disconnected");
                break;
            }

            var message = Encoding.UTF8.GetString(data, 0, bytesRead);
            Console.WriteLine("Received message: {0}", message);

            // Echo message back to client
            var response = Encoding.UTF8.GetBytes(message);
            try
            {
                stream.Write(response, 0, response.Length);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Error sending message: {0}", ex.Message);
                break;
            }
        }

        client.Close();
    }
}