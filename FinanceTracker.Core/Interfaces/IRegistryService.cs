namespace FinanceTracker.Core.Services
{
	public interface IRegistryService
	{
		void SetKeyLocation(string location);

		void AddSubPath(string key, string path);

		void SetSetting(string setting, string value);

		bool TryGetSetting<T>(string setting, out T value);

		void DeleteSetting(string setting);
	}
}
