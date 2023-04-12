using System.Net;
using System.Net.Sockets;
using System.Text;

namespace test;


public class ServerConnector
{

    public static async void SendToServers(List<string> ports)
    {
        try
        {
            const string fileName = "data/results.bin";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            var fileContent = File.ReadAllText(filePath);

            var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", ports[0]);
            await using var _stream = client.GetStream();
            var message = fileContent;
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length).ConfigureAwait(false);

        }
        catch (Exception e)
        {
            //Console.WriteLine(e.Message);
        }
    }
}