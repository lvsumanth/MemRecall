using MemRecall.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class LRUChainItemTests
    {
        [TestMethod]
        public void LRUChainItemTests_Constructor()
        {
            string str = "hello";
            var chainItem = new LRUChainItem<string>(value: str);
            Assert.AreEqual(str, chainItem.ItemValue);
        }
    }
}
