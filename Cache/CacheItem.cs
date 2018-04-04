using System;
using System.Collections.Generic;
using System.Text;

namespace FoneDynamics.Cache
{
    internal class CacheItem<T>
    {
        public T Data { get; set; }

        public DateTime LastRetrieved { get; set; } = DateTime.Now;
    }
}
