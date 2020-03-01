using MemRecall.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    public class LRUChainTests
    {
        private LRUChain<string> _chain;

        [TestInitialize]
        public void TestSetup()
        {
            _chain = new LRUChain<string>();
        }

        [TestMethod]
        public void LRUChainTests_ctor_initializes_empty_head_and_tail()
        {
            Assert.IsNull(_chain.TestEnabler_GetHead());
            Assert.IsNull(_chain.TestEnabler_GetTail());
        }

        [ExpectedException(exceptionType: typeof(ArgumentNullException), AllowDerivedTypes = false)]
        [TestMethod]
        public void LRUChainTests_AddFirst_throws_on_null()
        {
            _chain.AddFirst(null);
        }

        [TestMethod]
        public void LRUChainTests_AddFirst_only_one_item()
        {
            var item_1 = new LRUChainItem<string>("foo");

            _chain.AddFirst(item_1);

            Assert.AreEqual(item_1, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_1, _chain.TestEnabler_GetTail());
        }

        [TestMethod]
        public void LRUChainTests_AddFirst_two_items()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);

            Assert.AreEqual(item_2, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_1, _chain.TestEnabler_GetTail());
        }

        [TestMethod]
        public void LRUChainTests_AddFirst_three_items()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");
            var item_3 = new LRUChainItem<string>("who");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);
            _chain.AddFirst(item_3);

            Assert.AreEqual(item_3, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_1, _chain.TestEnabler_GetTail());
        }

        [ExpectedException(exceptionType: typeof(ArgumentNullException), AllowDerivedTypes = false)]
        [TestMethod]
        public void LRUChainTests_Promote_throws_on_null()
        {
            _chain.Promote(null);
        }

        [TestMethod]
        public void LRUChainTests_Promote_with_only_one_item()
        {
            var item_1 = new LRUChainItem<string>("foo");

            _chain.AddFirst(item_1);

            _chain.Promote(item_1);

            Assert.AreEqual(item_1, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_1, _chain.TestEnabler_GetTail());
        }

        [TestMethod]
        public void LRUChainTests_Promote_with_two_items_promote_head()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);

            _chain.Promote(item_2);

            Assert.AreEqual(item_2, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_1, _chain.TestEnabler_GetTail());
        }

        [TestMethod]
        public void LRUChainTests_Promote_with_two_items_promote_tail()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);

            _chain.Promote(item_1);

            Assert.AreEqual(item_1, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_2, _chain.TestEnabler_GetTail());
        }

        [TestMethod]
        public void LRUChainTests_Promote_with_three_items_promote_tail()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");
            var item_3 = new LRUChainItem<string>("who");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);
            _chain.AddFirst(item_3);

            _chain.Promote(item_1);

            Assert.AreEqual(item_1, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_2, _chain.TestEnabler_GetTail());
        }

        [TestMethod]
        public void LRUChainTests_Promote_with_three_items_promote_middle_item()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");
            var item_3 = new LRUChainItem<string>("who");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);
            _chain.AddFirst(item_3);

            _chain.Promote(item_2);

            Assert.AreEqual(item_2, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_1, _chain.TestEnabler_GetTail());
        }

        [TestMethod]
        public void LRUChainTests_GetItemToEvict_on_empty_list()
        {
            Assert.IsNull(_chain.GetItemToEvict());
        }

        [TestMethod]
        public void LRUChainTests_GetItemToEvict_on_list_with_one_item()
        {
            var item_1 = new LRUChainItem<string>("foo");

            _chain.AddFirst(item_1);

            Assert.AreEqual(item_1, _chain.GetItemToEvict());
        }

        [TestMethod]
        public void LRUChainTests_GetItemToEvict_on_list_with_two_items()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);

            Assert.AreEqual(item_1, _chain.GetItemToEvict());
        }

        [TestMethod]
        public void LRUChainTests_GetItemToEvict_on_list_with_three_items()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");
            var item_3 = new LRUChainItem<string>("who");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);
            _chain.AddFirst(item_3);

            Assert.AreEqual(item_1, _chain.GetItemToEvict());
        }

        [ExpectedException(exceptionType: typeof(ArgumentNullException), AllowDerivedTypes = false)]
        [TestMethod]
        public void LRUChainTests_Remove_throws_on_null()
        {
            _chain.Remove(null);
        }

        [TestMethod]
        public void LRUChainTests_Remove_when_only_one_item()
        {
            var item_1 = new LRUChainItem<string>("foo");

            _chain.AddFirst(item_1);

            _chain.Remove(item_1);

            Assert.IsNull(_chain.TestEnabler_GetHead());
            Assert.IsNull(_chain.TestEnabler_GetTail());
        }

        [TestMethod]
        public void LRUChainTests_Remove_head_when_two_items()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);

            // item_2 is the head
            _chain.Remove(item_2);

            Assert.AreEqual(item_1, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_1, _chain.TestEnabler_GetTail());
        }

        [TestMethod]
        public void LRUChainTests_Remove_tail_when_two_items()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);

            // item_2 is the head
            _chain.Remove(item_1);

            Assert.AreEqual(item_2, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_2, _chain.TestEnabler_GetTail());
        }

        [TestMethod]
        public void LRUChainTests_Remove_head_when_three_items()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");
            var item_3 = new LRUChainItem<string>("who");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);
            _chain.AddFirst(item_3);

            // order is 3, 2, 1
            _chain.Remove(item_3);

            Assert.AreEqual(item_2, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_1, _chain.TestEnabler_GetTail());
        }

        [TestMethod]
        public void LRUChainTests_Remove_middle_item_when_three_items()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");
            var item_3 = new LRUChainItem<string>("who");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);
            _chain.AddFirst(item_3);

            // order is 3, 2, 1
            _chain.Remove(item_2);

            Assert.AreEqual(item_3, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_1, _chain.TestEnabler_GetTail());
        }

        [TestMethod]
        public void LRUChainTests_Remove_tail_when_three_items()
        {
            var item_1 = new LRUChainItem<string>("foo");
            var item_2 = new LRUChainItem<string>("bar");
            var item_3 = new LRUChainItem<string>("who");

            _chain.AddFirst(item_1);
            _chain.AddFirst(item_2);
            _chain.AddFirst(item_3);

            // order is 3, 2, 1
            _chain.Remove(item_1);

            Assert.AreEqual(item_3, _chain.TestEnabler_GetHead());
            Assert.AreEqual(item_2, _chain.TestEnabler_GetTail());
        }
    }
}
