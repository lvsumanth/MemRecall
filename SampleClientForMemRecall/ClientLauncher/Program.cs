using SampleClientForMemRecall;
using System;
using System.Threading.Tasks;

namespace ClientLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            //CacheClient.RunSmallTestWithOneClient();

            Task.Run(() => CacheClient.RunParallelClients(numClients: 20, numInstructionsPerClient: 1000)).Wait();

            Console.Read();
        }
    }
}
