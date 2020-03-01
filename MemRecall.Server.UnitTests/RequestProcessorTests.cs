using MemRecall.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemRecall.Server.UnitTests
{
    [TestClass]
    public class RequestProcessorTests
    {
        private Mock<IConcurrentCache<string, string>> _mockCache;

        private RequestProcessor _processor;

        [TestInitialize]
        public void TestSetup()
        {
            _mockCache = new Mock<IConcurrentCache<string, string>>(MockBehavior.Strict);
            _processor = new RequestProcessor(_mockCache.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), AllowDerivedTypes = false)]
        public void RequestProcessorTests_ctor_throws_when_cache_null()
        {
            new RequestProcessor(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), AllowDerivedTypes = false)]

        public void RequestProcessorTests_ProcessRequest_throws_when_request_null()
        {
            _processor.ProcessRequest(null);
        }

        [TestMethod]
        public void RequestProcessorTests_ProcessRequest_malformed_request_doesnt_throw()
        {
            var req = new Request($"FooBar");
            _processor.ProcessRequest(req);

            // None of the cache methods should have been called.
            _mockCache.Verify(m => m.Set(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockCache.Verify(m => m.Get(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void RequestProcessorTests_ProcessRequest_valid_GET_multiple_keys_one_key_not_found()
        {
            string key_1 = "key_1";
            string value_1 = "value_1";

            string key_2 = "key_2";

            string key_3 = "key_3";
            string value_3 = "value_3";

            _mockCache
                .Setup(m => m.Get(key_1))
                .Returns(value_1);

            _mockCache
                .Setup(m => m.Get(key_2))
                .Returns((string)null);

            _mockCache
                .Setup(m => m.Get(key_3))
                .Returns(value_3);

            var req = new Request($"GET {key_1} {key_2} {key_3}");
            _processor.ProcessRequest(req);

            // Verify invocation of the mock methods.
            _mockCache.Verify(m => m.Set(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            _mockCache.Verify(m => m.Get(key_1), Times.Once);
            _mockCache.Verify(m => m.Get(key_2), Times.Once);
            _mockCache.Verify(m => m.Get(key_3), Times.Once);

            // Verify the dictionary in request - key_2 should have been removed.
            Assert.AreEqual(2, req.Data.Count);
            Assert.AreEqual(value_1, req.Data[key_1]);
            Assert.AreEqual(value_3, req.Data[key_3]);
        }

        [TestMethod]
        public void RequestProcessorTests_ProcessRequest_valid_SET()
        {
            string key_1 = "key_1";
            string value_1 = "value_1";

            _mockCache.Setup(m => m.Set(key_1, value_1));

            var req = new Request($"SET {key_1} 12 34 556\r\n{value_1}");
            _processor.ProcessRequest(req);

            // Verify invocation of the mock methods.
            _mockCache.Verify(m => m.Get(It.IsAny<string>()), Times.Never);
            _mockCache.Verify(m => m.Set(key_1, value_1), Times.Once);
            

            // Verify the dictionary in request
            Assert.AreEqual(1, req.Data.Count);
            Assert.AreEqual(value_1, req.Data[key_1]);
        }
    }
}
