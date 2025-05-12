using System.Net.Sockets;
using System.Text;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string serverIp = "localhost"; // Replace with PC 1's IP address
            int port = 5000;

            TcpClient client = new TcpClient();
            client.Connect(serverIp, port);
            Console.WriteLine("Connected to server.");
            Console.WriteLine("What is your username");
            string userName= Console.ReadLine();
            NetworkStream stream = client.GetStream();

            while (true)
            {
                Console.Write("Enter message (or 'exit'): ");
                string message = Console.ReadLine();
                if (message.ToLower() == "exit") break;

                byte[] data = Encoding.ASCII.GetBytes(userName + ": " + message);
                //we should do encryption
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                //Here we should do decryption
                Console.WriteLine("Server says: " + response);
            }

            stream.Close();
            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }
}
