namespace client;

using System;
using System.Net.Sockets;
using System.Text;


class Program
{
    static void Main(string[] args)
    {
        var servers = new [] { "0.0.0.0", "0.0.0.0", "0.0.0.0" };
        var serverIndex = 0;
        TcpClient client = null;

        while (true)
        {
            try
            {
                if (client == null)
                {
                    client = new TcpClient();
                    client.Connect(servers[serverIndex], 12345);
                    Console.WriteLine("Connected to server number: {0} ip: {1} ", serverIndex, servers[serverIndex]);
                }

                    Console.WriteLine("Enter your choice (rock, paper, scissors):");
                    var choice = Console.ReadLine();

                    var stream = client.GetStream();
                    var buffer = Encoding.UTF8.GetBytes(choice);
                    stream.Write(buffer, 0, buffer.Length);

                    buffer = new byte[1024];
                    var bytesRead = stream.Read(buffer, 0, buffer.Length);
                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var parts = message.Split(',');
                    var result = parts[0];
                    var serverChoice = parts[1];

                    Console.WriteLine($"Server chose {serverChoice}");
                    Console.WriteLine($"Result: {result}");

                    client.Close();
                client = null;
                serverIndex = (serverIndex + 1) % servers.Length;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                if (client != null)
                {
                    client.Close();
                    client = null;
                }

                serverIndex = (serverIndex + 1) % servers.Length;
            }
        }
    }
}




