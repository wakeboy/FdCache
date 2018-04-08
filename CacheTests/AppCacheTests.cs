using FluentAssertions;
using FoneDynamics.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FoneDynamics.CacheTests
{
    public class AppCacheTests
    {
        [Fact]
        public void Cannot_Construct_Cache_With_Negative_Capacity()
        {
            // Arrange
            Action cache = () => new AppCache(-8);

            // Act/Assert
            cache.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Cannot_Add_Item_With_Null_Key()
        {
            var cache = new AppCache(5);

            Action action = () => cache.AddOrUpdate(null, 5);

            action.Should().Throw<ArgumentException>();
        }


        [Fact]
        public void Can_Add_Items_To_Cache()
        {
            // Arrange
            ICache<string, object> cache = new AppCache(2);

            var key1 = "key1";
            var key2 = "key2";
            var val1 = 1;
            var val2 = 2;

            // Act
            cache.AddOrUpdate(key1, val1);
            cache.AddOrUpdate(key2, val2);

            object actual1;
            cache.TryGetValue(key1, out actual1);

            object actual2;
            cache.TryGetValue(key2, out actual2);

            // Assert 
            cache.Count.Should().Be(2);
            actual1.Should().Be(val1);
            actual2.Should().Be(val2);
        }

        [Fact]
        public void Can_Not_Overflow_Cache_Count()
        {
            ICache<string, object> cache = new AppCache(3);

            cache.AddOrUpdate("1", 1);
            cache.AddOrUpdate("2", 2);
            cache.AddOrUpdate("3", 3);
            cache.AddOrUpdate("4", 4);

            cache.Count.Should().Be(3);
        }

        [Fact]
        public void Eviction_Policy_Evicts_Item2()
        {
            ICache<string, object> cache = new AppCache(3);
            object item1Result;
            object item2Result;

            cache.AddOrUpdate("1", 1);
            cache.AddOrUpdate("2", 2);
            cache.AddOrUpdate("3", 3);
            cache.TryGetValue("1", out item1Result);
            cache.AddOrUpdate("4", 4);

            cache.TryGetValue("2", out item2Result);
            
            item2Result.Should().BeNull();
            cache.Count.Should().Be(3);
        }

        [Fact]
        public void Should_Not_Collided_When_Adding_Same_Key_From_Multiple_Treads()
        {
            ICache<string, object> cache = new AppCache(3);

            Task[] tasks = new Task[100];
            for (var i = 0; i < 100; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    cache.AddOrUpdate("1", 1);
                    Thread.Sleep(10);
                });
            }

            Task.WaitAll(tasks);
            
            cache.Count.Should().Be(1);

        }
    }
}
