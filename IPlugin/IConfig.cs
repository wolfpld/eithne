namespace Eithne
{
	internal interface IConfig
	{
		void Set(string key, string val);
		void Set(string key, int val);
		void Set(string key, bool val);

		string Get(string key, string def);
		int Get(string key, int def);
		bool Get(string key, bool def);
	}
}
