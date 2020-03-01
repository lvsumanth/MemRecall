using System;
using System.Collections.Generic;
using System.Text;

namespace MemRecall.Server
{
    /// <summary>
    /// Interface to capture the functionality of a request processor.
    /// </summary>
    public interface IRequestProcessor
    {
        /// <summary>
        /// Processes the specified request and updates the request instance with relevant data.
        /// </summary>
        /// <param name="request">The request to be processed.</param>
        void ProcessRequest(Request request);
    }
}
