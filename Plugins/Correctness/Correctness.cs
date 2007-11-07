using System;
using System.Reflection;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class CorrectnessInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("System Correctness"); }
		}

		public override string ShortName
		{
			get { return "Corr"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin shows correctness of system."); }
		}
	}

	public class CorrectnessFactory : IFactory
	{
		IInfo _info = new CorrectnessInfo();
		public IInfo Info
		{
			get { return _info; }
		}

		public IType Type
		{
			get { return IType.Out; }
		}

		public void Initialize()
		{
		}

		public Plugin.Base Create()
		{
			return new CorrectnessPlugin();
		}
	}

	public class Category
	{
		public Gdk.Pixbuf image = null;
		public int total = 0;
		public int matched = 0;
	}

	public class CorrectnessPlugin : Plugin.Out
	{
		int total = 0, matched = 0;
		Category[] cat = null;
		bool first = true;

		public CorrectnessPlugin()
		{
			_info = new CorrectnessInfo();
		}

		private void UpdateValue(bool val)
		{
			first = val;
			_block.Invalidate();
		}

		public override void Setup()
		{
			new CorrectnessSetup(UpdateValue, first);
		}

		public override bool HasSetup
		{
			get { return true; }
		}

		public override void DisplayResults()
		{
			if(!_workdone)
				throw new PluginException(Catalog.GetString("Plugin is not ready to display results."));

			new Correctness(total, matched, cat);
		}

		// it is assumed that categories are numbered in order, without holes
		private int FindNumCategories(int[] c)
		{
			int max = 0;

			for(int i=0; i<c.Length; i++)
				if(c[i] > max)
					max = c[i];

			return max;
		}

		public override void Work()
		{
			ICommResult r = _in[0] as ICommResult;
			int[] res = r.FindResultsSimple();
			int numcat = FindNumCategories(r.TestCategories);

			cat = new Category[numcat+1];
			for(int i=0; i<numcat+1; i++)
				cat[i] = new Category();

			total = r.Length;
			matched = 0;

			for(int i=0; i<r.Length; i++)
			{
				int tc = r.TestCategory(i);
				int bc = r.BaseCategory(res[i]);

				cat[tc].total++;

				if(cat[tc].image == null)
				{
					double scale;
					IImage img = null;

					for(int j=0; j<r.OriginalBaseImages.Length; j++)
						if(r.BaseCategory(j) == tc)
						{
							img = r.OriginalBaseImages[j];
							if(first)
								break;
						}

					if(img == null)
						cat[tc].image = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "no-base.png");
					else
					{
						if(img.W > img.H)
							scale = img.W / 48.0;
						else
							scale = img.H / 48.0;

						Gdk.Pixbuf tmp = img.CreatePixbuf();
						cat[tc].image = tmp.ScaleSimple(Scale(img.W, scale), Scale(img.H, scale),
								Gdk.InterpType.Bilinear);
					}
				}

				if(tc == bc && r.Match[i])
				{
					cat[tc].matched++;
					matched++;
				}
			}

			_workdone = true;
		}

		private int Scale(int s, double scale)
		{
			int val = (int)(s/scale);

			if(val == 0)
				return 1;
			else
				return val;
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");

			if(first)
				root.InnerText = "true";
			else
				root.InnerText = "false";
			
			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			if(root.InnerText == "true")
				first = true;
			else
				first = false;
		}

		public override int NumIn		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Results.");
		}

		private static string[] matchin  = new string[] { "result" };
		public override string[] MatchIn	{ get { return matchin; } }
	}
}
