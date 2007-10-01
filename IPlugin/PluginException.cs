using System;

namespace Eithne
{
	public class PluginException : Exception
	{
		public PluginException(string msg) : base(msg)
		{}
	}
}
