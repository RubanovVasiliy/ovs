namespace server;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


    class Program
    {
        static async Task Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 12345);
            listener.Start();

            Console.WriteLine("Server started");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected");

                Task.Run(() => HandleClient(client));
            }
        }

        static void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                string[] choices = new string[] { "rock", "paper", "scissors" };
                Random random = new Random();
                string serverChoice = choices[random.Next(choices.Length)];

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string clientChoice = Encoding.UTF8.GetString(buffer, 0, bytesRead);

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
                
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }



/*
using System.Net;
using System.Net.Sockets;
using System.Text;

IPAddress server1Address = IPAddress.Parse("192.168.0.1");
int server1Port = 8888;

// Создаем новый сокет для прослушивания входящих соединений на сервере 1
Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
listener.Bind(new IPEndPoint(server1Address, server1Port));
listener.Listen(100);

// Принимаем входящие соединения и запускаем новый поток для каждого клиента
while (true)
{
    Socket client = listener.Accept();
    Thread clientThread = new Thread(() => HandleClient(client));
    clientThread.Start();
}

// Обработка клиентского соединения
void HandleClient(Socket client)
{
    try
    {
        // Получаем данные от клиента
        byte[] buffer = new byte[1024];
        int bytesRead = client.Receive(buffer);
        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        // Сохраняем данные в базу данных или отправляем на другие сервера кластера
        SaveData(data);

        // Отправляем клиенту данные из базы данных или других серверов кластера
        string response = GetData();
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        client.Send(responseBytes);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
    finally
    {
        client.Shutdown(SocketShutdown.Both);
        client.Close();
    }
}

// Сохранение данных в базу данных или отправка на другие сервера кластера
void SaveData(string data)
{
    // Реализация механизма репликации данных между серверами кластера
}

// Получение данных из базы данных или других серверов кластера
string GetData()
{
// Реализ
}*/