using System;
using System.Runtime.Caching;

namespace Worldescape.Common
{
	public class CacheService : ICacheService
	{
		private ObjectCache Cache => MemoryCache.Default;

		public T Get<T>(string key)
		{
			return (T)Cache[key];
		}

		public void Set(string key, object data, int cacheTime = 0)
		{
			if (data == null)
			{
				return;
			}

			CacheItemPolicy policy = new CacheItemPolicy
			{
				AbsoluteExpiration = cacheTime > 0 ? DateTime.Now.AddMinutes(cacheTime) : ObjectCache.InfiniteAbsoluteExpiration,
			};

			Cache.Add(item: new CacheItem(key, data), policy: policy);
		}

		public bool IsSet(string key)
		{
			return Cache.Contains(key);
		}

		public void Remove(string key)
		{
			Cache.Remove(key);
		}

		public void Clear()
		{
			foreach (var item in Cache)
			{
				Remove(item.Key);
			}
		}
	}
}
