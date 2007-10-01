using System;
using System.IO;
using Microsoft.Win32;

namespace Eithne
{
	internal class RegistryConfig : IConfig
	{
		static string path = "Software/Eithne/";

		Config.Callback UpdateHandler;

		public RegistryConfig(Config.Callback UpdateHandler)
		{
			this.UpdateHandler = UpdateHandler;
		}

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

			UpdateHandler();
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

			UpdateHandler();
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

			UpdateHandler();
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
}
