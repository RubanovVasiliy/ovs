using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace test;

internal abstract class Program
{
    static async Task Main(string[] args)
    {
        var myPort = 8888;
        if (args.Length == 1) myPort = int.Parse(args[0]);

        var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
        var logger = loggerFactory.CreateLogger<TcpServer>();
        
        var redis = ConnectToRedis();
        
        var db = redis.GetDatabase();
        
        var server = new TcpServer(redis,db,logger);
        await server.ListenAsync(IPAddress.Parse("127.0.0.1"), myPort);
        await redis.CloseAsync();
        Environment.ExitCode = 0;
    }
    private static ConnectionMultiplexer ConnectToRedis()
    {
        while (true)
        {
            try
            {
                var redis = ConnectionMultiplexer.Connect("localhost");
                Console.WriteLine("Connected to Redis");
                return redis;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Thread.Sleep(5000);
            }
        }

    }
}

public class TcpServer
{
    private readonly ILogger<TcpServer> _logger;
    private readonly ConnectionMultiplexer _redisConnection;
    private readonly IDatabase _redisDatabase;

    public TcpServer(ConnectionMultiplexer redisConnection, IDatabase redisDatabase, ILogger<TcpServer> logger)
    {
        _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
        _redisDatabase = redisDatabase ?? throw new ArgumentNullException(nameof(redisDatabase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ListenAsync(IPAddress address, int port, CancellationToken cancellationToken = default)
    {
        var listener = new TcpListener(address, port);
        listener.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                _ = HandleClientAsync(client, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting client connection");
            }
        }

        listener.Stop();
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        var clientName = $"{remoteIpEndPoint?.Address}:{remoteIpEndPoint?.Port}";

        try
        {
            await using var stream = client.GetStream();
            Console.WriteLine("Client connected: {0}", clientName);

            var buffer = new byte[1024];
            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                    .ConfigureAwait(false);
                var clientChoice = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var choices = new[] { "rock", "paper", "scissors" };
                var random = new Random();
                var serverChoice = choices[random.Next(choices.Length)];
                var result = GetGameResult(clientChoice, serverChoice);
                await _redisDatabase.StringGetSetAsync(clientName, clientChoice).ConfigureAwait(false);
                
                //var response = await _redisDatabase.StringGetAsync(serverChoice).ConfigureAwait(false);
                //var responseData = Encoding.UTF8.GetBytes(response);
                //var message = $"{result},{serverChoice},{responseData}";

                var message = $"{result},{serverChoice}";

                var messageBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            client.Close();
            Console.WriteLine("Client disconnected: {0}", clientName);
        }
    }

    private static string GetGameResult(string clientChoice, string serverChoice)
    {
        if (clientChoice == serverChoice) return "tie";

        if ((clientChoice == "rock" && serverChoice == "scissors") ||
            (clientChoice == "paper" && serverChoice == "rock") ||
            (clientChoice == "scissors" && serverChoice == "paper"))
        {
            return "win";
        }

        return "lose";
    }
}