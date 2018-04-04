using System;
using System.Collections.Concurrent;
using System.Linq;

namespace FoneDynamics.Cache
{
    public class AppCache : ICache<string, object>
    {
        private readonly ConcurrentDictionary<string, CacheItem<object>> cache;
        private readonly int capacity;

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

            this.capacity = capacity;
            this.cache = new ConcurrentDictionary<string, CacheItem<object>>();
        }

        /// <summary>
        /// The number of items currently in the cache
        /// </summary>
        public int Count => this.cache.Count;

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

            if (cache.Count >= capacity)
            {
                RemoveOldest();
            }

            var cacheItem = new CacheItem<object> { Data = value };

            // Set the cache value with the cacheItem values
            this.cache.AddOrUpdate(key, cacheItem, (existingKey, existingValue) => {
                existingValue.Data = cacheItem.Data;
                existingValue.LastRetrieved = cacheItem.LastRetrieved;
                return existingValue;
            });
        }

        /// <summary>
        /// Attempts to gets the value from the cache against the specified key
        /// and returns true if the key existed in the cache.
        /// </summary>
        public bool TryGetValue(string key, out object value)
        {
            CacheItem<object> result;
            var found = this.cache.TryGetValue(key, out result);

            value = result?.Data;

            // Update cache with the last retrieved datetime stamp
            if (value != null)
            {
                AddOrUpdate(key, result.Data);
            }

            return found;
        }

        /// <summary>
        /// TODO: improved to 0(1) performance
        /// </summary>
        private void RemoveOldest()
        {
            var oldestKey = this.cache.OrderBy(item => item.Value.LastRetrieved).FirstOrDefault();

            CacheItem<object> removedItem;
            cache.TryRemove(oldestKey.Key, out removedItem);
        }
    }
}
