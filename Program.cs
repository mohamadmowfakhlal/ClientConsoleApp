using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpMultiClientClient
{
    class Program
    {
        static void Main()
        {
            string serverIp = "127.0.0.1";
            int port = 5000;

            try
            {
                TcpClient client = new TcpClient();
                client.Connect(serverIp, port);
                Console.WriteLine("Connected to server.");

                NetworkStream stream = client.GetStream();

                Console.Write("Enter your username: ");
                string userName = Console.ReadLine();

                // 🧵 Start background thread to listen to server messages
                Thread listenThread = new Thread(() => ListenToServer(stream));
                listenThread.IsBackground = true;
                listenThread.Start();

                // 📨 Main loop to send messages
                while (true)
                {
                    string message = Console.ReadLine();

                    if (message.ToLower() == "exit")
                        break;

                    string fullMessage = $"{userName}: {message}";
                    byte[] data = Encoding.UTF8.GetBytes(fullMessage);
                    stream.Write(data, 0, data.Length);
                }

                stream.Close();
                client.Close();
                Console.WriteLine("Disconnected from server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        // 🧠 Real-time receiving
        static void ListenToServer(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break; // Server closed connection

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"\n{message}");
                    Console.Write("> "); // Refresh prompt
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Disconnected from server: " + ex.Message);
            }
        }
    }
}
