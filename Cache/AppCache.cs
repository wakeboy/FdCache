using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FoneDynamics.Cache
{
    public class AppCache : ICache<string, object>
    {
        private object _syncLock = new object();

        // Index of cached keys, to keep track of the when items have been updated
        private readonly LinkedList<string> _cacheIndex;

        // Holds the cached items
        private readonly ConcurrentDictionary<string, object> _cache;

        // The maximum number of items which can be in the cache at any one time.
        private readonly int _capacity;

        /// <summary>
        /// Construct an instance of the cache class
        /// Use an IoC container to create a single(ton) instance of the AppCache
        /// </summary>
        /// <param name="capacity"></param>
        public AppCache(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException("capacity", "capacity must be greater than 0");
            }

            _capacity = capacity;
            _cache = new ConcurrentDictionary<string, object>();
            _cacheIndex = new LinkedList<string>();
        }

        /// <summary>
        /// The number of items currently in the cache
        /// </summary>
        public int Count {
            get
            {
                lock (_syncLock)
                {
                    return _cache.Count;
                }
            }
        } 

        /// <summary>
        /// Adds the value to the cache against the specified key.
        /// If the key already exists, its value is updated.
        /// </summary>
        public void AddOrUpdate(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key", "Cache key must not be null or empty");
            }

            if (Count >= _capacity)
            {
                EvictOldest();
            }

            if(!_cache.TryGetValue(key, out object existingValue))
            {
                _cache.TryAdd(key, value);
            }
            else
            {
                _cache.TryUpdate(key, value, existingValue);
            }

            UpdateCacheIndex(key);
        }

        /// <summary>
        /// Attempts to gets the value from the cache against the specified key
        /// and returns true if the key existed in the cache.
        /// </summary>
        public bool TryGetValue(string key, out object value)
        {
            var found = _cache.TryGetValue(key, out value);

            if (found)
            {
                UpdateCacheIndex(key);
            }

            return found;
        }

        /// <summary>
        /// Update the cache index; to keep track of which item was last accessed
        /// The most recent accessed item should always be last item in the array list.
        /// </summary>
        /// <param name="key">The Cache Index Key</param>
        public void UpdateCacheIndex(string key)
        {
            lock (_syncLock)
            {
                if (_cacheIndex.Contains(key))
                {
                    _cacheIndex.Remove(key);
                }
                _cacheIndex.AddLast(key);
            }
        }

        /// <summary>
        /// Remove the oldest item from the Cache and the Cache Index
        /// </summary>
        private void EvictOldest()
        {
            string oldestKey;
            lock (_syncLock)
            {
                oldestKey = _cacheIndex.First();
                _cacheIndex.Remove(oldestKey);
            }

            _cache.TryRemove(oldestKey, out object removedItem);
        }
    }
}
