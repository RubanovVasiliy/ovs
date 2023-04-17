using System.Net;
using System.Net.Sockets;
using System.Text;

namespace test;


public static class ServerConnector
{

    public static async void SendToServers(List<string> ports, string port)
    {
        try
        {
            var fileName = "data/" + port + ".bin";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            var fileContent = File.ReadAllText(filePath);
            var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", int.Parse(ports[0]));
            await using var stream = client.GetStream();
            var messageBytes = "log:"u8.ToArray();
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length).ConfigureAwait(false);
            
            messageBytes = Encoding.UTF8.GetBytes(fileContent);
            await stream.WriteAsync(messageBytes, 0, messageBytes.Length).ConfigureAwait(false);
            client.Close();
        }
        catch (Exception)
        {
            //Console.WriteLine(e.Message);
        }
    }
}