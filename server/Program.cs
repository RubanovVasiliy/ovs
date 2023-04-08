using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace test;

internal abstract class Program
{
    static async Task Main(string[] args)
    {
        var myPort = 8888;
        if (args.Length == 1) myPort = int.Parse(args[0]);

        var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
        var logger = loggerFactory.CreateLogger<TcpServer>();
        var server = new TcpServer(logger);
        await server.ListenAsync(IPAddress.Parse("127.0.0.1"), myPort);
        Environment.ExitCode = 0;
    }
}

public class TcpServer
{
    private readonly ILogger<TcpServer> _logger;


    public TcpServer( ILogger<TcpServer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ListenAsync(IPAddress address, int port, CancellationToken cancellationToken = default)
    {
        var listener = new TcpListener(address, port);
        listener.Start();
        _logger.LogInformation("Server started on PORT: {Port}", port);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
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
                var clientData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var res = clientData.Split(',');
                var id = res[0];
                var clientChoice = res[1];

                var choices = new[] { "rock", "paper", "scissors" };
                var random = new Random();
                var serverChoice = choices[random.Next(choices.Length)];
                var result = GetGameResult(clientChoice, serverChoice);
                string score;

                const string fileName = "data/results.bin";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                var fileContent = File.ReadAllText(filePath);

                var strings = fileContent.Split('\n');
                var resInFile = "";

                foreach (var str in strings)
                    if (str.Contains($"id:{id}"))
                        resInFile = str;

                if (!resInFile.Equals(""))
                {
                    var blocks = resInFile.Split(':');
                    var value = blocks[2];
                    value = SetScore(value, result);
                    fileContent = fileContent.Replace(resInFile, $"id:{id}:{value}");
                    File.WriteAllText(filePath, fileContent);
                    score = value;
                }
                else
                {
                    var value = SetScore("0,0,0", result);
                    await using (var writer = new StreamWriter(filePath, true))
                        await writer.WriteLineAsync($"id:{id}:{value}");
                    
                    score = value;
                }
                Console.WriteLine(GetLogAboutUser(id, clientChoice, score));
                var message = $"{result},{serverChoice}";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            //Console.WriteLine(e.Message);
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

    private static string SetScore(string score, string result)
    {
        var results = score.Split(',');
        var wins = int.Parse(results[0]);
        var loses = int.Parse(results[1]);
        var ties = int.Parse(results[2]);

        switch (result)
        {
            case "win":
                wins++;
                break;
            case "lose":
                loses++;
                break;
            case "tie":
                ties++;
                break;
        }

        return $"{wins},{loses},{ties}";
    }

    private static string GetLogAboutUser(string id, string choice, string score)
    {
        var results = score.Split(',');
        var wins = int.Parse(results[0]);
        var loses = int.Parse(results[1]);
        var ties = int.Parse(results[2]);

        return $"Client id: {id} client choice: {choice} Wins: {wins} Loses: {loses} Ties: {ties}";
    }
}