using System;
using System.IO;

namespace Eithne
{
	internal class GConfConfig : IConfig
	{
		GConf.Client c = new GConf.Client();
		Config.Callback UpdateCallback;

		static string path = "/apps/eithne/";

		public GConfConfig(Config.Callback UpdateCallback)
		{
			this.UpdateCallback = UpdateCallback;

			c.AddNotify(path.TrimEnd(new char[] {'/'}), UpdateHandler);
		}

		private void UpdateHandler(object o, EventArgs args)
		{
			UpdateCallback();
		}

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
}
