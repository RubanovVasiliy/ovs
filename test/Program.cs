using System.Net;
using System.Net.Sockets;
using System.Text;
using StackExchange.Redis;

namespace test;

internal abstract class Program
{
    static void Main(string[] args)
    {
        
        var myPort = 8888;

        
        ConnectionMultiplexer redis;

        while (true)
        {
            try
            {
                redis = ConnectionMultiplexer.Connect("localhost");
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Thread.Sleep(5000);
            }
        }

        var db = redis.GetDatabase();
        /*var sub = redis.GetSubscriber();
        
        sub.Subscribe("data", (channel, message) =>
        {
            Console.WriteLine("Received: " + message);
        });*/

        var server = new TcpServer("127.0.0.1", myPort);
        server.Start();
        server.DataReceived += (_, e) =>
        {
            db.StringSet(e.Client, e.Data);
            Console.WriteLine(e.Client + " " + e.Data);
            /*
            sub.Publish("data", e.Data);
        */
            
        };
        Console.ReadLine();
    }
}

class TcpServer
{
    private readonly string _ip;
    private readonly int _port;
    private TcpListener _listener;
    private bool _running;

    public TcpServer(string ip, int port)
    {
        _ip = ip;
        _port = port;
    }

    public void Start()
    {
        _listener = new TcpListener(IPAddress.Parse(_ip), _port);
        _listener.Start();
        _running = true;
        AcceptClients();
    }

    public void Stop()
    {
        _running = false;
        _listener.Stop();
    }

    private async void AcceptClients()
    {
        while (_running)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync();
                await Task.Run(() => ProcessClient(client));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    private async void ProcessClient(TcpClient client)
    {
        try
        {
            var remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            var clientName = $"{remoteIpEndPoint?.Address}:{remoteIpEndPoint?.Port}";
            Console.WriteLine("Client connected: {0}", clientName);
            await using var stream = client.GetStream();

            var buffer = new byte[1024];
            while (true)
            {

                var data = await stream.ReadAsync(buffer, 0, buffer.Length);
                var clientChoice = Encoding.UTF8.GetString(buffer, 0, data);

                if (clientChoice is "q") break;

                DataReceived?.Invoke(this, new DataReceivedEventArgs(clientName, clientChoice));
                
                string[] choices = new string[] { "rock", "paper", "scissors" };
                Random random = new Random();
                string serverChoice = choices[random.Next(choices.Length)];

                string result;
                if (clientChoice == serverChoice)
                {
                    result = "tie";
                }
                else if ((clientChoice == "rock" && serverChoice == "scissors") ||
                         (clientChoice == "paper" && serverChoice == "rock") ||
                         (clientChoice == "scissors" && serverChoice == "paper"))
                {
                    result = "win";
                }
                else
                {
                    result = "lose";
                }

                // Send result and server's choice
                string message = $"{result},{serverChoice}";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                stream.Write(messageBytes, 0, messageBytes.Length);
            }

            Console.WriteLine("Client disconnected: {0}", clientName);
            client.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            client.Close();
        }
    }

    public event EventHandler<DataReceivedEventArgs> DataReceived;
}

internal class DataReceivedEventArgs : EventArgs
{
    public DataReceivedEventArgs(string client, string data)
    {
        Data = data;
        Client = client;
    }

    public string Client { get; }
    public string Data { get; }
}