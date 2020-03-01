using System;
using System.Collections.Generic;
using System.Text;

namespace MemRecall.Core
{
    /// <summary>
    /// An interface representing a thread safe in-memory cache.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface IConcurrentCache<TKey, TValue>
    {
        /// <summary>
        /// Sets the specific value for the specified key.
        /// </summary>
        /// <param name="key">The key for which value needs to be set.</param>
        /// <param name="value">The value to be set.</param>
        void Set(TKey key, TValue value);

        /// <summary>
        /// Gets the value corresponding to the specified key.
        /// </summary>
        /// <param name="key">The key for which the value is to be retrieved.</param>
        /// <returns>The value for the specified key if found, null otherwise.</returns>
        TValue Get(TKey key);
    }
}
