using System;
using System.Collections.Generic;

namespace MemRecall.Core
{
    /// <summary>
    /// A thread safe in-memory cache with LRU eviction policy.
    /// </summary>
    public class ConcurrentLRUCache<TKey, TValue> : IConcurrentCache<TKey, TValue>
    {
        /// <summary>
        /// The lock to be used to serialize the access to the underlying dictionary.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// The underlying non-thread-safe dictionary.
        /// </summary>
        private Dictionary<TKey, LRUChainItem<KeyValuePair<TKey, TValue>>> _dictionary;

        /// <summary>
        /// The chain of objects used to track which are most recently usued. 
        /// The head of the chain always contains the most recently used and 
        /// the tail is always the least recently used.
        /// </summary>
        private LRUChain<KeyValuePair<TKey, TValue>> _lruChain;

        /// <summary>
        /// The capacity for the dictionary.
        /// </summary>
        private int _capacity;

        /// <summary>
        /// Creates an instance of ConcurrentLRUCache with the specified capacity.
        /// </summary>
        /// <param name="capacity">
        /// The capacity with which the dictionary needs to be initialized.
        /// </param>
        public ConcurrentLRUCache(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException($"'{nameof(capacity)}' must be greater than or equal to one.");
            }

            _capacity = capacity;

            // Initialize the dictionary with capacity to avoid resizing.
            _dictionary = new Dictionary<TKey, LRUChainItem<KeyValuePair<TKey, TValue>>>(capacity);

            _lruChain = new LRUChain<KeyValuePair<TKey, TValue>>();
        }

        /// <summary>
        /// Sets the specific value for the specified key.
        /// </summary>
        /// <param name="key">The key for which value needs to be set.</param>
        /// <param name="value">The value to be set.</param>
        public void Set(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException($"'{nameof(key)}' cannot be null.");
            }

            lock (_lock)
            {
                if (_dictionary.TryGetValue(
                        key: key,
                        value: out LRUChainItem<KeyValuePair<TKey, TValue>> lruChainItem)) // Key already present in cache.
                {
                    // Promote the LRU chain item and update the value in the chain item.
                    lruChainItem.ItemValue = new KeyValuePair<TKey, TValue>(key, value);
                    _lruChain.Promote(lruChainItem);

                }
                else // Key does not exist in the cache. Add fresh.
                {
                    // See if eviction is required.
                    EvictIfRequired();

                    // Set the item in the cache.
                    lruChainItem = new LRUChainItem<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key, value));
                    _dictionary[key] = lruChainItem;
                    _lruChain.AddFirst(lruChainItem);
                }
            }
        }

        /// <summary>
        /// Gets the value corresponding to the specified key.
        /// </summary>
        /// <param name="key">The key for which the value is to be retrieved.</param>
        /// <returns>The value for the specified key if found, null otherwise.</returns>
        public TValue Get(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException($"'{nameof(key)}' cannot be null.");
            }

            lock (_lock)
            {
                if (_dictionary.TryGetValue(
                         key: key,
                         value: out LRUChainItem<KeyValuePair<TKey, TValue>> lruChainItem)) // Key present in cache.
                {
                    // Promote the LRU chain item and return the value.
                    _lruChain.Promote(lruChainItem);
                    return lruChainItem.ItemValue.Value;

                }
                else // Key not found in cache.
                {
                    return default(TValue);
                }
            }
        }

        /// <summary>
        /// Checks the current size of the dictionary against capacity and evicts 
        /// the least recently used item if size equals capacity.
        /// </summary>
        private void EvictIfRequired()
        {
            if (_dictionary.Count < _capacity)
            {
                return;
            }

            var itemToEvict = _lruChain.GetItemToEvict();
            if (itemToEvict != null)
            {
                _dictionary.Remove(key: itemToEvict.ItemValue.Key);
                _lruChain.Remove(itemToEvict);
            }
        }
    }
}
