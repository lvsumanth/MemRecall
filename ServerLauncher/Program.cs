using MemRecall.Core;
using MemRecall.Diagnostics;
using MemRecall.Server;
using System.Net;

namespace ServerLauncher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ITelemetryLogger logger = new ConsoleLogger();
            IConcurrentCache<string, string> cache = new ConcurrentLRUCache<string, string>(capacity: 2048);

            IRequestProcessor processor = new RequestProcessor(cache); 
                        
            CacheServer server = new CacheServer(processor, logger);
            server.Start(ipAddress: IPAddress.Loopback, port: 11211);
        }
    }
}

