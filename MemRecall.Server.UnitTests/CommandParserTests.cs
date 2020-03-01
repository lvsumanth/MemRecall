using MemRecall.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MemRecall.Server.UnitTests
{
    [TestClass]
    public class CommandParserTests
    {
        private Mock<ITelemetryLogger> _mockLogger;
        private CommandParser _parser;
        private MemoryStream _stream;

        [TestInitialize]
        public void TestSetup()
        {
            _mockLogger = new Mock<ITelemetryLogger>(MockBehavior.Loose);
            _stream = new MemoryStream();
            _parser = new CommandParser(_stream, _mockLogger.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), AllowDerivedTypes = false)]
        public void CommandParser_ctor_throws_when_stream_null()
        {
            new CommandParser(stream: null, logger: _mockLogger.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), AllowDerivedTypes = false)]
        public void CommandParser_ctor_throws_when_stream_not_readable()
        {
            MockNonReadableStream stream = new MockNonReadableStream();

            new CommandParser(stream: stream, logger: _mockLogger.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), AllowDerivedTypes = false)]
        public void CommandParser_ctor_throws_when_logger_null()
        {
            new CommandParser(stream: new MemoryStream(), logger: null);
        }

        [TestMethod]
        public async Task CommandParser_GetRequestsFromStream_returns_empty_when_complete_instruction_not_received()
        {
            var cmdBytes = Encoding.Unicode.GetBytes($"GET Ke");

            _stream.Write(cmdBytes, 0, cmdBytes.Length);

            var requests = await _parser.GetRequestsFromStream();

            Assert.AreEqual(0, requests.Count);
        }

        [TestMethod]
        public async Task CommandParser_GetRequestsFromStream_returns_1_req_when_exactly_1_req_received()
        {
            string key_1 = "Key_one";
            var cmdBytes = Encoding.Unicode.GetBytes($"GET {key_1}\r\n");

            _stream.Write(cmdBytes, 0, cmdBytes.Length);

            // When the reader is presented with the network stream,
            // the seek position would be start of the stream.
            _stream.Seek(0, SeekOrigin.Begin);

            var requests = await _parser.GetRequestsFromStream();

            Assert.AreEqual(1, requests.Count);
            Assert.AreEqual(RequestCommand.Get, requests[0].Command);
            Assert.AreEqual(1, requests[0].Data.Count);
            Assert.AreEqual(null, requests[0].Data[key_1]);
        }

        [TestMethod]
        public async Task CommandParser_GetRequestsFromStream_returns_1_req_when_slightly_more_than_1_req_received()
        {
            string key_1 = "Key_one";
            var cmdBytes = Encoding.Unicode.GetBytes($"GET {key_1}\r\nGET");

            _stream.Write(cmdBytes, 0, cmdBytes.Length);

            // When the reader is presented with the network stream,
            // the seek position would be start of the stream.
            _stream.Seek(0, SeekOrigin.Begin);

            var requests = await _parser.GetRequestsFromStream();

            Assert.AreEqual(1, requests.Count);
            Assert.AreEqual(RequestCommand.Get, requests[0].Command);
            Assert.AreEqual(1, requests[0].Data.Count);
            Assert.AreEqual(null, requests[0].Data[key_1]);
        }

        [TestMethod]
        public async Task CommandParser_GetRequestsFromStream_returns_correctly_compond_scenario()
        {
            var cmdBytes = Encoding.Unicode.GetBytes($"GET key_1\r\nGET key_2 key_3\r\nSET Key_5");
            _stream.Write(cmdBytes, 0, cmdBytes.Length);

            long lastOffset = _stream.Position;

            // When the reader is presented with the network stream,
            // the seek position would be start of the stream.
            _stream.Seek(0, SeekOrigin.Begin);

            var requests = await _parser.GetRequestsFromStream();

            Assert.AreEqual(2, requests.Count);
            Assert.AreEqual(RequestCommand.Get, requests[0].Command);
            Assert.AreEqual(RequestCommand.Get, requests[1].Command);

            // Move back to the stream where we left off writing.
            _stream.Seek(lastOffset, SeekOrigin.Begin);

            // Continue writing the remainder of the SET command to the stream.
            cmdBytes = Encoding.Unicode.GetBytes($" 12 32 334\r\nvalue_for_key_5\r\nGET K");
            _stream.Write(cmdBytes, 0, cmdBytes.Length);

            // Seek back to where the reader would have left reading.
            _stream.Seek(lastOffset, SeekOrigin.Begin);

            requests = await _parser.GetRequestsFromStream();
            Assert.AreEqual(1, requests.Count);
            Assert.AreEqual(RequestCommand.Set, requests[0].Command);
        }

        /// <summary>
        /// A mock stream which returns 'CanRead' as false
        /// </summary>
        class MockNonReadableStream : Stream
        {
            public override bool CanRead => false;

            public override bool CanSeek => throw new NotImplementedException();

            public override bool CanWrite => throw new NotImplementedException();

            public override long Length => throw new NotImplementedException();

            public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }
    }
}
