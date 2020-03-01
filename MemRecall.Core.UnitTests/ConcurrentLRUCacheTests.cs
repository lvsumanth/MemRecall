using MemRecall.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MemRecall.UnitTests
{
    [TestClass]
    public class ConcurrentLRUCacheTests
    {
        private ConcurrentLRUCache<string, string> _cache;

        [TestInitialize]
        public void TestSetup()
        {
            _cache = new ConcurrentLRUCache<string, string>(capacity: 5);
        }

        [TestMethod]
        public void ConcurrentLRUCacheTests_Get_on_non_existent_key_returns_null()
        {
            Assert.IsNull(_cache.Get(key: "foo"));

            _cache.Set(key: "abc", value: "random value");

            Assert.IsNull(_cache.Get(key: "foo"));
        }
        
        [TestMethod]
        public void ConcurrentLRUCacheTests_Get_on_existent_key_returns_value()
        {
            var testKey = "testkey";
            var testValue = "testvalue";


            _cache.Set(key: "abc", value: "random value");
            _cache.Set(key: testKey, value: testValue);

            Assert.AreEqual(testValue, _cache.Get(testKey));
        }

        [TestMethod]
        public void ConcurrentLRUCacheTests_Get_returns_null_when_key_evicted()
        {
            // We've initialized the capacity to 5.

            var key_1 = Guid.NewGuid().ToString();
            var value_1 = Guid.NewGuid().ToString();

            // After setting the first key, do not perform a 'get' on 
            // that key to verify the value. Doing so will promote it
            // in the LRU engine and cause it to not get evicted.

            // First key set.
            _cache.Set(key: key_1, value: value_1);

            // Set five more keys
            for (int i=0; i<5; i++)
            {
                _cache.Set(key: Guid.NewGuid().ToString(), value: Guid.NewGuid().ToString());
            }

            // The first key should be evicted by now.
            Assert.IsNull(_cache.Get(key_1));
        }

        [TestMethod]
        public void ConcurrentLRUCacheTests_Get_results_in_promotion_within_LRU_enging()
        {
            // We've initialized the capacity to 5.

            var key_1 = Guid.NewGuid().ToString();
            var value_1 = Guid.NewGuid().ToString();

            var key_2 = Guid.NewGuid().ToString();
            var value_2 = Guid.NewGuid().ToString();

            // After setting the second key, we will perform a get on the 
            // first key to promote it in the LRU engine over the second 
            // key. So, when the sixth key gets inserted, it will be the 
            // second key that gets evicted, not the first. 

            // Set first key.
            _cache.Set(key: key_1, value: value_1);

            // Set the second key.
            _cache.Set(key: key_2, value: value_2);

            // Read first key but not the second key.
            Assert.AreEqual(value_1, _cache.Get(key_1));
            
            // Insert 4 more keys. 
            for (int i = 0; i < 4; i++)
            {
                _cache.Set(key: Guid.NewGuid().ToString(), value: Guid.NewGuid().ToString());
            }

            // The first key should NOT be evicted.
            Assert.AreEqual(value_1, _cache.Get(key_1));

            // The second key should be evicted.
            Assert.IsNull(_cache.Get(key_2));
        }

        [TestMethod]
        public async Task ConcurrentLRUCacheTests_concurrent_executions_dont_cause_deadlocks()
        {
            List<Task> tasksToAwaitOn = new List<Task>();

            for (int i=0; i<1000; i++)
            {
                tasksToAwaitOn.Add(
                    Task.Run(() =>
                        {
                            for (int j=0; j<10000; j++)
                            {
                                var key = Guid.NewGuid().ToString();
                                var value = Guid.NewGuid().ToString();

                                _cache.Set(key, value);
                                _cache.Get(key);                                
                            }
                        }));
            }

            await Task.WhenAll(tasksToAwaitOn);
        }
    }
}
