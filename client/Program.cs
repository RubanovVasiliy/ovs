namespace test_client;

internal abstract class TcpClientExample
{
    static void Main(string[] args)
    {
        var myPort = 15732;
        if (args.Length == 1) myPort = int.Parse(args[0]);
        
        var chatClient = new ChatClient(myPort);
        chatClient.SendMessage();
    }
}