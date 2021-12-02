namespace Worldescape.Common
{
	public static class CacheExtensions
	{
		public static T Get<T>(this ICacheService cacheManager, string key, T acquire)
		{
			return Get(cacheManager, key, 60, acquire);
		}

		public static T Get<T>(this ICacheService cacheManager, string key, int cacheTime, T acquire)
		{
			if (cacheManager.IsSet(key))
			{
				return cacheManager.Get<T>(key);
			}
			else
			{
				var result = acquire;

				cacheManager.Set(key, result, cacheTime);

				return result;
			}
		}
	}
}
