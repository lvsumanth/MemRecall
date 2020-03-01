using System;
using System.Collections.Generic;
using System.Text;

namespace MemRecall.Diagnostics
{
    /// <summary>
    /// Logs the diagnostics messages to the default console.
    /// </summary>
    public class ConsoleLogger : ITelemetryLogger
    {
        /// <summary>
        /// Logs a verbose trace line.
        /// </summary>
        /// <param name="str">The string to log.</param>
        public void TraceVerbose(string str)
        {
            Console.WriteLine(str);
        }
    }
}
