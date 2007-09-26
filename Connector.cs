using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class ConnectorInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Connector"); }
		}

		public override string ShortName
		{
			get { return " "; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("Connector between blocks."); }
		}
	}

	public class ConnectorFactory : IFactory
	{
		IInfo _info = new ConnectorInfo();
		public IInfo Info
		{
			get { return _info; }
		}

		public IType Type
		{
			get { return IType.Other; }
		}

		public void Initialize()
		{
		}

		public IPlugin Create()
		{
			return new ConnectorPlugin();
		}
	}

	public class ConnectorPlugin : IOtherPlugin
	{
		private static string[] matchin  = new string[] { "" };
		private static string[] matchout = new string[] { "" };

		public ConnectorPlugin()
		{
			_info = new ConnectorInfo();
		}

		public override void Setup()
		{
		}

		public override bool HasSetup
		{
			get { return false; }
		}

		public override void Work()
		{
			_out = new CommSocket(1);

			_out[0] = _in[0];

			_workdone = true;
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Input signal.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Copied signal.");
		}

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }
	}
}
