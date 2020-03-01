using MemRecall.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemRecall.Server
{
    /// <summary>
    /// Processes a request.
    /// </summary>
    public class RequestProcessor : IRequestProcessor
    {
        /// <summary>
        /// The cache from which the requests will be served.
        /// </summary>
        private IConcurrentCache<string, string> _cache = null;

        /// <summary>
        /// Creates an instance of RequestProcessor.
        /// </summary>
        /// <param name="cache">The cache from which the requests will be served.</param>
        public RequestProcessor(IConcurrentCache<string, string> cache)
        {
            _cache = cache ?? throw new ArgumentNullException($"{nameof(cache)} cannot be null.");
        }

        /// <summary>
        /// Processes the specified request and updates the request instance with relevant data.
        /// </summary>
        /// <param name="request">The request to be processed.</param>
        public void ProcessRequest(Request request)
        {
            if (request == null)
            {
                throw new ArgumentNullException($"{nameof(request)} cannot be null.");
            }

            if (request.IsMalformed)
            {
                return;
            }

            if (request.Command == RequestCommand.Set)
            {
                foreach (var kvp in request.Data)
                {
                    _cache.Set(key: kvp.Key, value: kvp.Value);
                }
            }
            else if (request.Command == RequestCommand.Get)
            {
                var keysToProcesses = request.Data.Keys.ToList();
                var keysToRemove = new List<string>();
                foreach (var key in keysToProcesses)
                {
                    string value = _cache.Get(key);
                    if (value == null)
                    {
                        keysToRemove.Add(key);
                    }
                    else
                    {
                        request.Data[key] = value;
                    }
                }

                foreach (var key in keysToRemove)
                {
                    request.Data.Remove(key);
                }
            }
        }
    }
}
