// ============================
// CLIENT SIDE (Hybrid Encryption)
// ============================

using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

class SecureClient
{
    static byte[] aesKey;

    static void Main()
    {
        TcpClient client = new TcpClient("127.0.0.1", 5000);
        NetworkStream stream = client.GetStream();

        // Step 1: Receive RSA public key from server
        byte[] buffer = new byte[2048];
        int keyLength = stream.Read(buffer, 0, buffer.Length);
        string serverPublicKey = Encoding.UTF8.GetString(buffer, 0, keyLength);

        // Step 2: Generate AES key and send encrypted version
        Aes aes = Aes.Create();
        aesKey = aes.Key;
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(serverPublicKey);
        byte[] encryptedKey = rsa.Encrypt(aesKey, false);
        stream.Write(encryptedKey, 0, encryptedKey.Length);

        // Start thread to listen to incoming messages
        new Thread(() => ListenToServer(stream)).Start();

        Console.Write("Enter your username: ");
        string username = Console.ReadLine();

        while (true)
        {
            string input = Console.ReadLine();
            if (input.ToLower() == "exit") break;

            string message = username + ": " + input;
            using Aes aesLocal = Aes.Create();
            aesLocal.Key = aesKey;
            aesLocal.IV = new byte[16];
            ICryptoTransform encryptor = aesLocal.CreateEncryptor();
            byte[] encryptedMsg = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(message), 0, message.Length);

            stream.Write(encryptedMsg, 0, encryptedMsg.Length);
        }
    }

    static void ListenToServer(NetworkStream stream)
    {
        byte[] buffer = new byte[2048];
        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                using Aes aes = Aes.Create();
                aes.Key = aesKey;
                aes.IV = new byte[16];
                ICryptoTransform decryptor = aes.CreateDecryptor();
                string message = Encoding.UTF8.GetString(decryptor.TransformFinalBlock(buffer, 0, bytesRead));
                Console.WriteLine("\n" + message);
                Console.Write("> ");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Disconnected: " + ex.Message);
        }
    }
}
