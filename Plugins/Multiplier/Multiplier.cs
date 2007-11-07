using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class MultiplierInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Multiplier"); }
		}

		public override string ShortName
		{
			get { return "Mul"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin multiplies input signal on several output sockets."); }
		}
	}

	public class MultiplierFactory : IFactory
	{
		IInfo _info = new MultiplierInfo();
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

		public Plugin.Base Create()
		{
			return new MultiplierPlugin();
		}
	}

	public class MultiplierPlugin : Plugin.Other
	{
		private int num = 2;

		public MultiplierPlugin()
		{
			_info = new MultiplierInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private void UpdateValue(int n)
		{
			num = n;
			_block.SlotsChanged();
		}

		public override void Setup()
		{
			new MultiplierSetup(num, UpdateValue);
		}

		public override void Work()
		{
			_out = new CommSocket(num);

			for(int i=0; i<num; i++)
				_out[i] = _in[0];

			_workdone = true;
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");

			root.InnerText = num.ToString();
			
			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			UpdateValue(Int32.Parse(root.InnerText));
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return num; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Input signal.");
		}

		public override string DescOut(int n)
		{
			return String.Format(Catalog.GetString("{0}. copy of signal."), n+1);
		}

		private static string[] matchin  = new string[] { "" };
		private static string[] matchout = new string[] { "image/rgb", "image/grayscale", "image/float", "result" };

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }
	}
}
