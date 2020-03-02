using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public static void RunSmallTestWithOneClient()
        {
            try
            {
                using (var client = new TcpClient())
                {
                    Console.WriteLine("Connecting to server...");
                    client.Connect(IPAddress.Loopback, 11211);
                    Console.WriteLine("Connected!\r\n");

                    using (NetworkStream networkStream = client.GetStream())
                    {
                        Stopwatch stopWatch = new Stopwatch();

                        stopWatch.Start();
                        WriteData("GET KEY_1\r\n", networkStream, emitTraces: true);
                        Console.WriteLine("-- Expecting an 'END' response with no data.");
                        ReadData(networkStream, emitTraces: true);
                        Console.WriteLine($"-- Received response in {stopWatch.Elapsed.TotalMilliseconds} ms.\r\n");

                        stopWatch.Restart();
                        WriteData("GET KEY_2\r\n", networkStream, emitTraces: true);
                        Console.WriteLine("-- Expecting an 'END' response with no data.");
                        ReadData(networkStream, emitTraces: true);
                        Console.WriteLine($"-- Received response in {stopWatch.Elapsed.TotalMilliseconds} ms.\r\n");

                        stopWatch.Restart();
                        WriteData("SET KEY_1 34 43 112\r\nValue_for_key_1\r\n", networkStream, emitTraces: true);
                        Console.WriteLine("-- Expecting a 'STORED' response with no data.");
                        ReadData(networkStream, emitTraces: true);
                        Console.WriteLine($"-- Received response in {stopWatch.Elapsed.TotalMilliseconds} ms.\r\n");

                        stopWatch.Restart();
                        WriteData("GET KEY_1\r\n", networkStream, emitTraces: true);
                        Console.WriteLine("-- Expecting a VALUE response with key and its value.");
                        ReadData(networkStream, emitTraces: true);
                        Console.WriteLine($"-- Received response in {stopWatch.Elapsed.TotalMilliseconds} ms.\r\n");

                        stopWatch.Restart();
                        WriteData("SET KEY_2 34 43 112\r\nValue_for_key_2\r\n", networkStream, emitTraces: true);
                        Console.WriteLine("-- Expecting a 'STORED' response with no data.");
                        ReadData(networkStream, emitTraces: true);
                        Console.WriteLine($"-- Received response in {stopWatch.Elapsed.TotalMilliseconds} ms.\r\n");

                        stopWatch.Restart();
                        WriteData("GET KEY_1 KEY_2\r\n", networkStream, emitTraces: true);
                        Console.WriteLine("-- Expecting a VALUE response for both keys and their values.");
                        ReadData(networkStream, emitTraces: true);
                        Console.WriteLine($"-- Received response in {stopWatch.Elapsed.TotalMilliseconds} ms.\r\n");
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
        /// Run the specified number of clients in parallel, each client issuing the specified
        /// number of instructions.
        /// </summary>
        public static async Task RunParallelClients(int numClients, int numInstructionsPerClient)
        {
            List<Task<double>> tasksToAwaitOn = new List<Task<double>>();

            for (int i=0; i<numClients; i++)
            {
                tasksToAwaitOn.Add(Task.Run(() =>
                {
                    return ExecuteClient(numInstructionsPerClient);
                }));
            }

            await Task.WhenAll(tasksToAwaitOn);

            List<double> latencies = new List<double>(numClients);
            tasksToAwaitOn.ForEach(task => latencies.Add(task.Result));
            Console.WriteLine($"Average latency seen is: {latencies.Average()}ms.");
        }

        /// <summary>
        /// Executes the specified numner of instructions using one client.
        /// </summary>
        /// <returns>
        /// The average latency the client has experienced, in milliseconds.
        /// </returns>
        public static double ExecuteClient(int numInstructions)
        {
            try
            {
                List<double> latencies = new List<double>(numInstructions + 10);

                using (var client = new TcpClient())
                {
                    client.Connect(IPAddress.Loopback, 11211);
                    using (NetworkStream networkStream = client.GetStream())
                    {
                        string keyPrefix = $"KEY_{Guid.NewGuid().ToString()}_";

                        Stopwatch stopWatch = new Stopwatch();
                        Random random = new Random();

                        // First set values on all the keys
                        for (int i=0; i<numInstructions/2; i++)
                        {
                            string key = $"{keyPrefix}{i}";

                            stopWatch.Restart();

                            // Send the request
                            WriteData($"SET {key} 34 43 112\r\nValue_for_{key}\r\n", networkStream, emitTraces: false);

                            // Wait for the response
                            ReadData(networkStream, emitTraces: false);

                            latencies.Add(stopWatch.Elapsed.TotalMilliseconds);

                            int waitTimeInMilliseconds = (int) random.NextDouble() * 10;
                            Thread.Sleep(millisecondsTimeout: waitTimeInMilliseconds);
                        }

                        // Then try to read all the keys
                        for (int i = 0; i < numInstructions / 2; i++)
                        {
                            string key = $"{keyPrefix}{i}";

                            stopWatch.Restart();

                            // Send the request
                            WriteData($"GET {key}\r\n", networkStream, emitTraces: false);

                            // Wait for the response
                            ReadData(networkStream, emitTraces: false);

                            latencies.Add(stopWatch.Elapsed.TotalMilliseconds);

                            int waitTimeInMilliseconds = (int)random.NextDouble() * 10;
                            Thread.Sleep(millisecondsTimeout: waitTimeInMilliseconds);
                        }

                        stopWatch.Stop();
                    }

                    client.Close();
                }

                return latencies.Average();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }

            return -1;
        }

        /// <summary>
        /// Writes the data to the specified stream using UTF-16 encoding.
        /// </summary>
        private static void WriteData(string data, NetworkStream stream, bool emitTraces = false)
        {
            if (emitTraces)
            {
                Console.Write($"-- Sending Request:\r\n{data}");
            }
            
            Byte[] byteData = Encoding.Unicode.GetBytes(data);
            stream.Write(byteData, 0, byteData.Length);
        }

        /// <summary>
        /// Reads the data from the incoming network stream.
        /// </summary>
        private static string ReadData(NetworkStream stream, bool emitTraces = false)
        {
            var receivedBuffer = new byte[2048];
            var numberOfBytesRead = stream.Read(receivedBuffer, 0, receivedBuffer.Length);

            if (numberOfBytesRead == 0)
            {
                return string.Empty;
            }

            var response = Encoding.Unicode.GetString(receivedBuffer, 0, numberOfBytesRead);

            if (emitTraces)
            {
                Console.Write($"-- Received Response:\r\n{response}");
            }

            return response;
        }
    }
}
