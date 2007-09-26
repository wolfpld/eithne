using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class XofYInfo : IInfo
	{
		private int X = 2, Y = 3;

		public void UpdateX(int x)
		{
			X = x;
		}

		public void UpdateY(int y)
		{
			Y = y;
		}

		public override string Name
		{
			get { return Catalog.GetString("X of Y"); }
		}

		public override string ShortName
		{
			get { return String.Format("{0}/{1}", X, Y); }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin selects images when X of Y match."); }
		}
	}

	public class XofYFactory : IFactory
	{
		IInfo _info = new XofYInfo();
		public IInfo Info
		{
			get { return _info; }
		}

		public IType Type
		{
			get { return IType.ResProc; }
		}

		public void Initialize()
		{
		}

		public IPlugin Create()
		{
			return new XofYPlugin();
		}
	}

	public class XofYPlugin : IResProcPlugin
	{
		XofYInfo _info = new XofYInfo();
		private int x = 2;
		private int y = 3;

		public XofYPlugin()
		{
		}

		public override IInfo Info
		{
			get { return _info; }
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private void UpdateValue(int _x, int _y)
		{
			x = _x;
			y = _y;

			_info.UpdateX(_x);
			_info.UpdateY(_y);

			_block.SlotsChanged();
		}

		public override void Setup()
		{
			new XofYSetup(x, y, UpdateValue);
		}

		public override void Work()
		{
			ICommResult ires = _in[0] as ICommResult;

			int tcount = ires.Length;
			int bcount = ires[0].Length;

			int[][] res = new int[y][];

			for(int i=0; i<y; i++)
			{
				ICommResult r = _in[i] as ICommResult;

				if(r.Length != tcount || r[0].Length != bcount)
					throw new PluginException(Catalog.GetString("Incompatible data on input."));

				res[i] = Utility.FindResultsSimple(r);
			}

			bool[] match = new bool[tcount];

			for(int i=0; i<tcount; i++)
				match[i] = true;

			IResult[] resarray = new IResult[tcount];

			for(int i=0; i<tcount; i++)
			{
				double[] tmp = new double[bcount];
				int[] cnt = new int[bcount];
				int j;

				for(j=0; j<bcount; j++)
				{
					tmp[j] = 0;
					cnt[j] = 0;
				}

				for(j=0; j<y; j++)
					if((_in[j] as ICommResult).Match[i])
						cnt[res[j][i]]++;

				for(j=0; j<bcount; j++)
					if(cnt[j] >= x)
					{
						tmp[j] = cnt[j];
						break;
					}

				// brak dopasowania
				if(j == bcount)
					match[i] = false;

				resarray[i] = new IResult(tmp);
			}

			_out = new CommSocket(1);
			_out[0] = new ICommResult(resarray, y, ires.OriginalBaseImages, ires.OriginalTestImages, ires.BaseCategories,
					ires.TestCategories, match);


			_workdone = true;
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");

			XmlNode n = _xmldoc.CreateNode(XmlNodeType.Element, "x", "");
			n.InnerText = x.ToString();
			root.AppendChild(n);

			n = _xmldoc.CreateNode(XmlNodeType.Element, "y", "");
			n.InnerText = y.ToString();
			root.AppendChild(n);
			
			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			int x, y;

			XmlNode n = root.SelectSingleNode("x");
			x = Int32.Parse(n.InnerText);

			n = root.SelectSingleNode("y");
			y = Int32.Parse(n.InnerText);

			UpdateValue(x, y);
		}

		public override int NumIn		{ get { return y; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return String.Format(Catalog.GetString("{0}. input signal."), n+1);
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Best results.");
		}

		private static string[] matchin   = new string[] { "result" };
		private static string[] matchout  = new string[] { "result/processed" };

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }
	}
}
