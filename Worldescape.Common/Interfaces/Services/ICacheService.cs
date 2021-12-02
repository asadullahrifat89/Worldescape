namespace Worldescape.Common
{
	public interface ICacheService
	{
		T Get<T>(string key);

		void Set(string key, object data, int cacheTime = 0);

		bool IsSet(string key);

		void Remove(string key);

		void Clear();
	}
}
