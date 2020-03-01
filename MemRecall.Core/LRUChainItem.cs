using System;
using System.Collections.Generic;
using System.Text;

namespace MemRecall.Core
{
    /// <summary>
    /// Represents an item in the chain maintained by the LRU engine. 
    /// </summary>
    /// <typeparam name="TValue">The type of the value encapsulated by an instance of this item.</typeparam>
    public class LRUChainItem<TValue>
    {
        /// <summary>
        /// Creates an instance of <see cref="LRUChainItem"/>.
        /// </summary>
        /// <param name="value">The value encapsulated by this instance.</param>
        public LRUChainItem(TValue value)
        {
            ItemValue = value;
        }

        /// <summary>
        /// Gets or sets the value wrapped by the item instance.
        /// </summary>
        public TValue ItemValue { get; set; }

        /// <summary>
        /// Gets or sets the previous item in the chain.
        /// </summary>
        public LRUChainItem<TValue> Previous { get; set; }

        /// <summary>
        /// Gets or sets the next item in the chain.
        /// </summary>
        public LRUChainItem<TValue> Next { get; set; }
    }
}
