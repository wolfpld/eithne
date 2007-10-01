using System;
using System.IO;

namespace Eithne
{
	public class Config
	{
		public delegate void Callback();

		private static IConfig cfg = null;

		public static void Init(Callback UpdateHandler)
		{
			try
			{
				cfg = new GConfConfig(UpdateHandler);
			}
			catch(Exception e)
			{
				Console.WriteLine("Cannot use GConf based config.");
				Console.WriteLine(e.Message);
				Console.WriteLine("Trying to fall back to registry based config.");

				cfg = new RegistryConfig(UpdateHandler);
			}
		}

		public static void Set(string key, string val)
		{
			cfg.Set(key, val);
		}

		public static void Set(string key, int val)
		{
			cfg.Set(key, val);
		}
		public static void Set(string key, bool val)
		{
			cfg.Set(key, val);
		}

		public static string Get(string key, string def)
		{
			return cfg.Get(key, def);
		}

		public static int Get(string key, int def)
		{
			return cfg.Get(key, def);
		}

		public static bool Get(string key, bool def)
		{
			return cfg.Get(key, def);
		}
	}
}
