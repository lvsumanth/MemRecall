using MemRecall.Diagnostics;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MemRecall.Server
{
    /// <summary>
    /// A cache server implementing a TCP listener.
    /// </summary>
    public class CacheServer
    {
        /// <summary>
        /// The processor to be used to process the requests.
        /// </summary>
        private IRequestProcessor _processor;

        /// <summary>
        /// The diagnostics logger.
        /// </summary>
        private ITelemetryLogger _logger;

        /// <summary>
        /// Creates an instance of CacheServer.
        /// </summary>
        public CacheServer(
            IRequestProcessor processor,
            ITelemetryLogger logger)
        {
            _processor = processor ?? throw new ArgumentNullException($"{nameof(processor)} cannot be null.");
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)} cannot be null.");
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <param name="ipAddress">The IPAddress.</param>
        /// <param name="port">The port number to listen to.</param>
        public void Start(IPAddress ipAddress, int port)
        {
            TcpListener tcpListener = null;

            try
            {
                tcpListener = new TcpListener(ipAddress, port);
                tcpListener.Start();

                while (true)
                {
                    _logger.TraceVerbose("Server (Main thread): Waiting for a connection...");

                    TcpClient client = tcpListener.AcceptTcpClient();

                    _logger.TraceVerbose("Server (Main thread): Connected!");

                    Task.Run(() => HandleTcpClient(client));
                }
            }
            catch (SocketException e)
            {
                _logger.TraceVerbose($"Server (Main thread): SocketException - {e.Message}");
            }
            finally
            {
                _logger.TraceVerbose($"Server (Main thread): Stopping TCP Server...");
                tcpListener?.Stop();
                _logger.TraceVerbose($"Server (Main thread): TCP Server stopped!");
            }
        }

        /// <summary>
        /// Routine used to handle a TCP connection to a client and process the requests coming via that connection.
        /// </summary>
        private async Task HandleTcpClient(TcpClient client)
        {
            using (var networkStream = client.GetStream())
            {
                if (!networkStream.CanWrite)
                {
                    throw new InvalidOperationException($"{nameof(networkStream)} is not writeable.");
                }

                var parser = new CommandParser(networkStream, _logger);

                while (client.Connected)
                {
                    var requests = await parser.GetRequestsFromStream();

                    foreach(var req in requests)
                    {
                        // Process the request. 
                        _processor.ProcessRequest(req);

                        // Write the response to the stream.
                        await networkStream.WriteAsync(Encoding.Unicode.GetBytes(req.GetResponseText()));
                    }
                }

                _logger.TraceVerbose($"Client disconnected!");
            }
        }
    }
}
