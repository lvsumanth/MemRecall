using System;
using System.Collections.Generic;
using System.Text;

namespace MemRecall.Diagnostics
{
    /// <summary>
    /// Interface to capture the functionality of a telemetry logger.
    /// </summary>
    public interface ITelemetryLogger
    {
        /// <summary>
        /// Logs a verbose trace line.
        /// </summary>
        /// <param name="str">The string to log.</param>
        void TraceVerbose(string str);

        // Other variants of the method go here.
    }
}
