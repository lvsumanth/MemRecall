using System.Collections.Generic;
using System.Text;

namespace MemRecall.Server
{
    /// <summary>
    /// Class representing a request from a client.
    /// </summary>
    public class Request
    {
        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="commandText">The command text representing the command.</param>
        public Request(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                IsMalformed = true;
                return;
            }

            if (commandText.StartsWith("GET"))
            {
                ParseGet(commandText);
            }
            else if (commandText.StartsWith("SET"))
            {
                ParseSet(commandText);
            }
            else
            {
                IsMalformed = true;
            }
        }

        /// <summary>
        /// Specifies the type of the command.
        /// </summary>
        public RequestCommand Command { get; private set; }

        /// <summary>
        /// The data associated with the command.
        /// </summary>
        /// <remarks>
        /// For GET, before the request is processed, only the keys will be populated.
        /// After processing, the values corresponding to the keys will get populated. 
        /// If a key is not found, then that key will be removed from the dictionary.
        /// 
        /// For SET, before the request is processed, both key and value will be populated.
        /// The data will not change after processing the request.
        /// </remarks>
        public Dictionary<string, string> Data { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Indicates if the request is malformed.
        /// </summary>
        public bool IsMalformed { get; private set; } = false;

        /// <summary>
        /// After the request has been process, gets the text to be sent as response.
        /// </summary>
        /// <returns>The response text.</returns>
        public string GetResponseText()
        {
            StringBuilder responseBuilder = new StringBuilder();

            if (IsMalformed)
            {
                responseBuilder.Append("ERROR");
                responseBuilder.Append(Constants.CommandDelimiter);

            }
            else if (Command == RequestCommand.Set)
            {
                responseBuilder.Append("STORED");
                responseBuilder.Append(Constants.CommandDelimiter);
            }
            else
            {
                foreach (var kvp in Data)
                {
                    responseBuilder.Append("VALUE ");
                    responseBuilder.Append(kvp.Key);
                    responseBuilder.Append(" ");
                    responseBuilder.Append(kvp.Value.Length);
                    responseBuilder.Append(Constants.CommandDelimiter);
                    responseBuilder.Append(kvp.Value);
                    responseBuilder.Append(Constants.CommandDelimiter);
                }

                responseBuilder.Append("END");
                responseBuilder.Append(Constants.CommandDelimiter);
            }

            return responseBuilder.ToString();
        }

        /// <summary>
        /// Parses the command text to interpret a GET request.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        private void ParseGet(string commandText)
        {
            /*
             * Command text for the request will be of the shape: 
             * 
             * GET KEY_1 KEY_2 KEY_3\r\n
             * 
             * Response text for the request will be off the shape:
             * 
             * VALUE KEY_1 flags bytes\r\n
             * some_random_value_for_key_1\r\n
             * VALUE KEY_3 flags bytes\r\n
             * some_random_value_for_key_3\r\n
             * END\r\n
             * 
             */

            // There should be no \r\n in the command and data string. 
            if (commandText.IndexOf(Constants.CommandDelimiter) >= 0)
            {
                IsMalformed = true;
                return;
            }

            var tokens = commandText.Split(' ');

            if (tokens.Length < 2)
            {
                IsMalformed = true;
                return;
            }

            if (tokens[0] != "GET")
            {
                IsMalformed = true;
                return;
            }

            Command = RequestCommand.Get;

            for (int i = 1; i < tokens.Length; i++)
            {
                Data[tokens[i]] = null;
            }
        }

        /// <summary>
        /// Parses the string to interpret a SET command.
        /// </summary>
        /// <param name="commandAndDataText">The command and data text.</param>
        private void ParseSet(string commandAndDataText)
        {
            /*
             * Command text for the request will be of the shape: 
             * 
             * SET KEY_1 561 8872 38\r\n     // the numbers are flags, expiry and num_bytes
             * some_random_value_for_key_1\r\n
             * 
             * Response text for the request will be off the shape:
             * 
             * STORED\r\n       // all other values are used for CAS (check and set)
             */

            var lines = commandAndDataText.Split(Constants.CommandDelimiter);
            if (lines.Length != 2)
            {
                IsMalformed = true;
                return;
            }

            // Process the command text
            var cmdTextTokens = lines[0].Split(' ');
            if (cmdTextTokens.Length < 2)
            {
                IsMalformed = true;
                return;
            }

            if (cmdTextTokens[0] != "SET")
            {
                IsMalformed = true;
                return;
            }

            Command = RequestCommand.Set;
            Data[cmdTextTokens[1]] = lines[1];
        }
    }

    /// <summary>
    /// The type of the command.
    /// </summary>
    public enum RequestCommand
    {
        Unknown,
        Get,
        Set
    }
}
