using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SampleClientForMemRecall
{
    /// <summary>
    /// A sample/test client to be used with the MemRecall caching server.
    /// </summary>
    public static class CacheClient
    {
        /// <summary>
        /// Sends a small number of well known requests and displays the response from the server.
        /// </summary>
        public static void RunSmallTest()
        {
            try
            {
                using (var client = new TcpClient())
                {
                    Console.WriteLine("Connecting to server...");
                    client.Connect(IPAddress.Loopback, 11211);
                    Console.WriteLine("Connected!\r\n\r\n\r\n");

                    using (NetworkStream networkStream = client.GetStream())
                    {
                        Stopwatch stopWatch = new Stopwatch();

                        stopWatch.Start();
                        WriteData("GET KEY_1\r\n", networkStream);
                        Console.WriteLine("-- Expecting an 'END' response with no data. --\r\n");
                        ReadData(networkStream);
                        Console.WriteLine($"-- Received response in {stopWatch.Elapsed.TotalMilliseconds} ms. --\r\n");

                        stopWatch.Restart();
                        WriteData("GET KEY_2\r\n", networkStream);
                        Console.WriteLine("-- Expecting an 'END' response with no data. --\r\n");
                        ReadData(networkStream);
                        Console.WriteLine($"-- Received response in {stopWatch.Elapsed.TotalMilliseconds} ms. --\r\n");

                        stopWatch.Restart();
                        WriteData("SET KEY_1 34 43 112\r\nValue_for_key_1\r\n", networkStream);
                        Console.WriteLine("-- Expecting a 'STORED' response with no data. --\r\n");
                        ReadData(networkStream);
                        Console.WriteLine($"-- Received response in {stopWatch.Elapsed.TotalMilliseconds} ms. --\r\n");

                        stopWatch.Restart();
                        WriteData("GET KEY_1\r\n", networkStream);
                        Console.WriteLine("-- Expecting a VALUE response with key and its value. --\r\n");
                        ReadData(networkStream);
                        Console.WriteLine($"-- Received response in {stopWatch.Elapsed.TotalMilliseconds} ms. --\r\n");

                        stopWatch.Restart();
                        WriteData("SET KEY_2 34 43 112\r\nValue_for_key_2\r\n", networkStream);
                        Console.WriteLine("-- Expecting a 'STORED' response with no data. --\r\n");
                        ReadData(networkStream);
                        Console.WriteLine($"-- Received response in {stopWatch.Elapsed.TotalMilliseconds} ms. --\r\n");

                        stopWatch.Restart();
                        WriteData("GET KEY_1 KEY_2\r\n", networkStream);
                        Console.WriteLine("-- Expecting a VALUE response for both keys and their values. --\r\n");
                        ReadData(networkStream);
                        Console.WriteLine($"-- Received response in {stopWatch.Elapsed.TotalMilliseconds} ms. --\r\n");
                    }

                    client.Close();
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }

        /// <summary>
        /// Writes the data to the specified stream using UTF-16 encoding.
        /// </summary>
        private static void WriteData(string data, NetworkStream stream)
        {
            Console.WriteLine($"Sending Request:\r\n{data}");
            Byte[] byteData = Encoding.Unicode.GetBytes(data);
            stream.Write(byteData, 0, byteData.Length);
        }

        /// <summary>
        /// Reads the data from the incoming network stream.
        /// </summary>
        private static void ReadData(NetworkStream stream)
        {
            var receivedBuffer = new byte[2048];
            var numberOfBytesRead = stream.Read(receivedBuffer, 0, receivedBuffer.Length);

            if (numberOfBytesRead == 0)
            {
                return;
            }

            Console.WriteLine($"Received Response:\r\n\r\n" + Encoding.Unicode.GetString(receivedBuffer, 0, numberOfBytesRead));
        }
    }
}
