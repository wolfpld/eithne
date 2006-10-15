using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class BestInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Best-of"); }
		}

		public override string ShortName
		{
			get { return "Best"; }
		}

		public override string Version
		{
			get { return "0.1"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin selects best of results."); }
		}
	}

	public class BestFactory : IFactory
	{
		IInfo _info = new BestInfo();
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
			return new BestPlugin();
		}
	}

	public class BestPlugin : IResProcPlugin
	{
		private int num = 3;

		public BestPlugin()
		{
			_info = new BestInfo();
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
			new BestSetup(num, UpdateValue);
		}

		public override void Work()
		{
			int tcount = (_in[0] as ICommResult).Length;
			int bcount = (_in[0] as ICommResult)[0].Length;

			double[][] points = new double[tcount][];
			for(int i=0; i<tcount; i++)
			{
				points[i] = new double[bcount];
				for(int j=0; j<bcount; j++)
					points[i][j] = 0;
			}

			for(int i=0; i<num; i++)
			{
				ICommResult r = _in[i] as ICommResult;

				if(r.Length != tcount || r[0].Length != bcount)
					throw new PluginException(Catalog.GetString("Incompatible data on input."));

				// FIXME dorobić konfigurację
				int[][] res = Utility.FindResults(r);

				for(int t=0; t<tcount; t++)
				{
					points[t][res[t][0]] += 1;

					if(res[t].Length > 1)
						points[t][res[t][1]] += 0.5;

					if(res[t].Length > 2)
						points[t][res[t][2]] += 0.25;
				}
			}

			IResult[] resarray = new IResult[tcount];

			for(int i=0; i<tcount; i++)
				resarray[i] = new IResult(points[i]);

			_out = new CommSocket(1);
			_out[0] = new ICommResult(resarray, num, (_in[0] as ICommResult).OriginalBaseImages,
					(_in[0] as ICommResult).OriginalTestImages, (_in[0] as ICommResult).BaseCategories,
					(_in[0] as ICommResult).TestCategories);

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

		public override int NumIn		{ get { return num; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return String.Format(Catalog.GetString("{0}. input signal."), n+1);
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Best results.");
		}
	}
}
