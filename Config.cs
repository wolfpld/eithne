using System;
using System.IO;
using Microsoft.Win32;

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

	internal class GConfConfig : IConfig
	{
		GConf.Client c = new GConf.Client();

		static string path = "/apps/eithne/";

		public void Set(string key, string val)
		{
			c.Set(path + key, val);
		}

		public void Set(string key, int val)
		{
			c.Set(path + key, val);
		}

		public void Set(string key, bool val)
		{
			c.Set(path + key, val);
		}

		public string Get(string key, string def)
		{
			try
			{
				return (string)c.Get(path + key);
			}
			catch(GConf.NoSuchKeyException)
			{
				c.Set(path + key, def);
				return def;
			}
		}

		public int Get(string key, int def)
		{
			try
			{
				return (int)c.Get(path + key);
			}
			catch(GConf.NoSuchKeyException)
			{
				c.Set(path + key, def);
				return def;
			}
		}

		public bool Get(string key, bool def)
		{
			try
			{
				return (bool)c.Get(path + key);
			}
			catch(GConf.NoSuchKeyException)
			{
				c.Set(path + key, def);
				return def;
			}
		}
	}

	internal class RegistryConfig : IConfig
	{
		static string path = "Software/Eithne/";

		private string[] Split(string key)
		{
			string[] ret = new string[2];

			ret[0] = Path.GetDirectoryName(path + key);
			ret[0].Replace("/", "\\");

			ret[1] = Path.GetFileName(key);

			return ret;
		}

		public void Set(string key, string val)
		{
			string[] r = Split(key);

			RegistryKey k = Registry.CurrentUser.OpenSubKey(r[0], true);

			if(k == null)
				k = Registry.CurrentUser.CreateSubKey(r[0]);

			k.SetValue(r[1], val);

			k.Flush();
			k.Close();
		}

		public void Set(string key, int val)
		{
			string[] r = Split(key);

			RegistryKey k = Registry.CurrentUser.OpenSubKey(r[0], true);

			if(k == null)
				k = Registry.CurrentUser.CreateSubKey(r[0]);

			k.SetValue(r[1], val);

			k.Flush();
			k.Close();
		}

		public void Set(string key, bool val)
		{
			string[] r = Split(key);

			RegistryKey k = Registry.CurrentUser.OpenSubKey(r[0], true);

			if(k == null)
				k = Registry.CurrentUser.CreateSubKey(r[0]);

			if(val)
				k.SetValue(r[1], 1);
			else
				k.SetValue(r[1], 0);

			k.Flush();
			k.Close();
		}

		public string Get(string key, string def)
		{
			string[] r = Split(key);

			RegistryKey k = Registry.CurrentUser.OpenSubKey(r[0], true);

			if(k == null)
				k = Registry.CurrentUser.CreateSubKey(r[0]);

			string val = (string)k.GetValue(r[1]);

			if(val == null)
			{
				k.SetValue(r[1], def);
				val = def;
			}

			k.Flush();
			k.Close();

			return val;
		}

		public int Get(string key, int def)
		{
			string[] r = Split(key);

			RegistryKey k = Registry.CurrentUser.OpenSubKey(r[0], true);

			if(k == null)
				k = Registry.CurrentUser.CreateSubKey(r[0]);

			object val = k.GetValue(r[1]);

			if(val == null)
			{
				k.SetValue(r[1], def);
				val = def;
			}

			k.Flush();
			k.Close();

			return (int)val;
		}

		public bool Get(string key, bool def)
		{
			string[] r = Split(key);

			RegistryKey k = Registry.CurrentUser.OpenSubKey(r[0], true);

			if(k == null)
				k = Registry.CurrentUser.CreateSubKey(r[0]);

			object val = k.GetValue(r[1]);

			bool ret;

			if(val == null)
			{
				if(def)
					k.SetValue(r[1], 1);
				else
					k.SetValue(r[1], 0);

				ret = def;
			}
			else
				if((int)val == 1)
					ret = true;
				else
					ret = false;

			k.Flush();
			k.Close();

			return ret;
		}
	}

	public class Config
	{
		private static IConfig cfg = null;

		public static void Init()
		{
			try
			{
				cfg = new GConfConfig();
			}
			catch(Exception e)
			{
				Console.WriteLine("Cannot use GConf based config.");
				Console.WriteLine(e.Message);
				Console.WriteLine("Trying to fall back to registry based config.");

				cfg = new RegistryConfig();
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
