# FdCache

A simple in memory cache

The cache should be used wit your IoC container to register a single instance, 
so items are not cached miltiple times.

## Example Usage Autofac: 

int cacheCapacity = 20; 
builder.RegisterInstance(new AppCache(cacheCapacity)).As<ICache<string, object>>(); 

### Code Coverage 

