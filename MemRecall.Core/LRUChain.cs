using System;
using System.Collections.Generic;
using System.Text;

namespace MemRecall.Core
{
    /// <summary>
    /// Represents the abstraction used to encapsulate the doubly linked list used 
    /// to maintain the least recently used information on a collection of items.
    /// </summary>
    public class LRUChain<TValue>
    {
        /// <summary>
        /// The head of the doubly linked list.
        /// </summary>
        private LRUChainItem<TValue> _head = null;

        /// <summary>
        /// The tail of the doubly linked list.
        /// </summary>
        private LRUChainItem<TValue> _tail = null;

        /// <summary>
        /// Gets the current head.
        /// </summary>
        /// <remarks>
        /// TO BE USED FOR UNIT TESTING PURPOSES ONLY.
        /// </remarks>
        internal LRUChainItem<TValue> TestEnabler_GetHead()
        {
            return _head;
        }

        /// <summary>
        /// Gets the current tail.
        /// </summary>
        /// <remarks>
        /// TO BE USED FOR UNIT TESTING PURPOSES ONLY.
        /// </remarks>
        internal LRUChainItem<TValue> TestEnabler_GetTail()
        {
            return _tail;
        }

        /// <summary>
        /// Adds item to the front of the list.
        /// </summary>
        public void AddFirst(LRUChainItem<TValue> item)
        {
            if (item == null)
            {
                throw new ArgumentNullException($"'{nameof(item)}' cannot be null");
            }

            if (_head == null) // case of empty list
            {
                item.Previous = null;
                item.Next = null;                

                _head = item;
                _tail = item;
            }
            else
            {
                // Create a link between the item and _head.
                item.Previous = null;
                item.Next = _head;
                _head.Previous = item;

                _head = item;
                // Tail doesn't update.
            }
        }
        
        /// <summary>
        /// Promotes the item to the front of the list.
        /// </summary>
        public void Promote(LRUChainItem<TValue> item)
        {
            if (item == null)
            {
                throw new ArgumentNullException($"'{nameof(item)}' cannot be null");
            }

            if (item == _head)
            {
                // Already at the front of the list. No action requried.
                return;
            }

            Remove(item);
            AddFirst(item);
        }

        /// <summary>
        /// Gets the least recently read item (last in the list).
        /// </summary>
        public LRUChainItem<TValue> GetItemToEvict()
        {
            return _tail;
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        public void Remove(LRUChainItem<TValue> item)
        {
            if (item == null)
            {
                throw new ArgumentNullException($"'{nameof(item)}' cannot be null");
            }

            // Update the link between the previous and next items.
            if (item.Previous != null)
            {
                item.Previous.Next = item.Next;
            }
            if (item.Next != null)
            {
                item.Next.Previous = item.Previous;
            }

            // Check if _tail needs to be updated.
            if (_tail == item)
            {
                _tail = item.Previous;
            }

            // Check if _head needs to be updated.
            if (_head == item)
            {
                _head = item.Next;
            }
        }
    }
}
