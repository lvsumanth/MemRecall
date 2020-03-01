using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MemRecall.Server.UnitTests
{
    [TestClass]
    public class RequestTests
    {
        [TestMethod]
        public void RequestTests_null_cmd_text_sets_malformed()
        {
            Request req = new Request(null);
            Assert.IsTrue(req.IsMalformed);
            Assert.AreEqual(RequestCommand.Unknown, req.Command);
        }

        [TestMethod]
        public void RequestTests_empty_cmd_text_sets_malformed()
        {
            Request req = new Request(string.Empty);
            Assert.IsTrue(req.IsMalformed);
            Assert.AreEqual(RequestCommand.Unknown, req.Command);
        }

        [TestMethod]
        public void RequestTests_gibberish_cmd_text_sets_malformed()
        {
            Request req = new Request("lorem ipsum lorem impsum");
            Assert.IsTrue(req.IsMalformed);
            Assert.AreEqual(RequestCommand.Unknown, req.Command);
        }

        [TestMethod]
        public void RequestTests_invalid_GET()
        {
            Request req = new Request($"GET \r\nkey_1 ");

            Assert.IsTrue(req.IsMalformed);
            Assert.AreEqual(RequestCommand.Unknown, req.Command);
        }

        [TestMethod]
        public void RequestTests_valid_GET_single_key()
        {
            string key = "key_1";
            Request req = new Request($"GET {key}");

            Assert.IsFalse(req.IsMalformed);
            Assert.AreEqual(RequestCommand.Get, req.Command);
            Assert.IsNotNull(req.Data);
            Assert.AreEqual(1, req.Data.Count);
            Assert.IsNull(req.Data[key]);
        }

        [TestMethod]
        public void RequestTests_valid_GET_three_keys()
        {
            string key_1 = "key_1";
            string key_2 = "key_2";
            string key_3 = "key_3";

            Request req = new Request($"GET {key_1} {key_2} {key_3}");

            Assert.IsFalse(req.IsMalformed);
            Assert.AreEqual(RequestCommand.Get, req.Command);
            Assert.IsNotNull(req.Data);
            Assert.AreEqual(3, req.Data.Count);
            Assert.IsNull(req.Data[key_1]);
            Assert.IsNull(req.Data[key_2]);
            Assert.IsNull(req.Data[key_3]);
        }
                
        [TestMethod]
        public void RequestTests_valid_SET()
        {
            string key = "Key_1";
            string value = "Value_1";
            Request req = new Request($"SET {key} 32 34 883\r\n{value}");

            Assert.IsFalse(req.IsMalformed);
            Assert.AreEqual(RequestCommand.Set, req.Command);
            Assert.IsNotNull(req.Data);
            Assert.AreEqual(1, req.Data.Count);
            Assert.AreEqual(value, req.Data[key]);
        }

        [TestMethod]
        public void RequestTests_invalid_SET()
        {
            Request req = new Request($"SET KEY 32 34 883\r");

            Assert.IsTrue(req.IsMalformed);
            Assert.AreEqual(RequestCommand.Unknown, req.Command);
        }

        [TestMethod]
        public void RequestTests_GetResponseText_invalid()
        {
            Request req = new Request($"GET \r\nkey_1 ");
            Assert.AreEqual("ERROR\r\n", req.GetResponseText());
        }

        [TestMethod]
        public void RequestTests_GetResponseText_valid_GET_no_keys_found()
        {
            Request req = new Request($"GET key_1");

            req.Data.Remove("key_1");

            Assert.AreEqual("END\r\n", req.GetResponseText());
        }

        [TestMethod]
        public void RequestTests_GetResponseText_valid_GET_multiple_keys()
        {
            string key_1 = "key_1";
            string value_1 = "value_1";

            string key_2 = "key_2";
            string value_2 = "value_2";

            string key_3 = "key_3";
            string value_3 = "value_3";

            Request req = new Request($"GET {key_1} {key_2} {key_3}");

            req.Data[key_1] = value_1;
            req.Data[key_2] = value_2;
            req.Data[key_3] = value_3;

            string expectedResponse = $"VALUE {key_1} 7\r\n{value_1}\r\nVALUE {key_2} 7\r\n{value_2}\r\nVALUE {key_3} 7\r\n{value_3}\r\nEND\r\n";
            Assert.AreEqual(expectedResponse, req.GetResponseText());
        }

        [TestMethod]
        public void RequestTests_GetResponseText_valid_SET()
        {
            Request req = new Request($"SET key_1\r\nfoobar");

            Assert.AreEqual("STORED\r\n", req.GetResponseText());
        }
    }
}
