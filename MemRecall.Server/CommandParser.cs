using MemRecall.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MemRecall.Server
{
    /// <summary>
    /// Parses the incoming network stream to generate 'request' abstractions.
    /// </summary>
    public class CommandParser
    {
        /// <summary>
        /// The instance to be used for diagnostic logging.
        /// </summary>
        private ITelemetryLogger _logger;

        /// <summary>
        /// The stream containing command data.
        /// </summary>
        private Stream _stream;

        /// <summary>
        /// The unprocesses segment of the incoming request stream.
        /// </summary>
        string _unprocessedSegment = string.Empty;

        /// <summary>
        /// Creates an instance of CommandParser.
        /// </summary>
        /// <param name="stream">Stream containing command data.</param>
        /// <param name="logger">An instance of diagnostic logger.</param>
        public CommandParser(Stream stream, ITelemetryLogger logger)
        {
            _stream = stream ?? throw new ArgumentNullException($"{nameof(stream)} cannot be null.");

            if (!_stream.CanRead)
            {
                throw new InvalidOperationException($"{nameof(stream)} is not readable.");
            }
            
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)} cannot be null."); ;
        }

        /// <summary>
        /// Gets one of more request commands from the stream.
        /// </summary>
        /// <returns>A list of request commands.</returns>
        public async Task<List<Request>> GetRequestsFromStream()
        {
            byte[] readBuffer = new byte[2048];
            int numberOfBytesRead = 0;

            List<Request> requests = new List<Request>();

            // Read whatever is available in the network buffer.
            numberOfBytesRead = await _stream.ReadAsync(readBuffer, 0, readBuffer.Length);

            if (numberOfBytesRead == 0)
            {
                return requests;
            }

            // Append the newly read data to what has already been buffered (but not processed)
            _unprocessedSegment += Encoding.Unicode.GetString(readBuffer, 0, numberOfBytesRead);

            // Check if a complete request is available.
            int delimiterIndex = GetDelimiterIndex(startIndex: 0);

            while (delimiterIndex >= 0)
            {
                if (_unprocessedSegment.StartsWith("SET"))
                {
                    // We need data up until the next delimiter to process the SET command. 
                    delimiterIndex = GetDelimiterIndex(startIndex: delimiterIndex + Constants.CommandDelimiter.Length);
                    if (delimiterIndex < 0)
                    {
                        // Not enough data available yet to process SET.
                        // Wait for full data to become available.
                        break;
                    }
                }

                // We expect only one delimiter for GET command. 
                requests.Add(new Request(_unprocessedSegment.Substring(0, delimiterIndex)));

                // If there is more to process, update 'unprocessedSegment'
                _unprocessedSegment = _unprocessedSegment.Substring(delimiterIndex + Constants.CommandDelimiter.Length);

                delimiterIndex = GetDelimiterIndex(startIndex: 0);
            }

            return requests;
        }

        /// <summary>
        /// Gets the 0-based index of the first occurence of the compound delimiter '\r\n' in '_unprocessedSegment'.
        /// </summary>
        /// <param name="startIndex">The index from which the search needs to begin.</param>
        /// <returns>0-based index of the first occurence of the compound delimiter '\r\n', -1 otherwise.</returns>
        private int GetDelimiterIndex(int startIndex)
        {
            return _unprocessedSegment.IndexOf(Constants.CommandDelimiter, startIndex);
        }
    }
}
