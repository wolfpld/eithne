using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class L0ModInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Modified L<sub>0</sub> metric"); }
		}

		public override string ShortName
		{
			get { return "L0mod"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin calculates modified L<sub>0</sub> metric between images."); }
		}
	}

	public class L0ModFactory : IFactory
	{
		IInfo _info = new L0ModInfo();
		public IInfo Info
		{
			get { return _info; }
		}

		public IType Type
		{
			get { return IType.Comparator; }
		}

		public void Initialize()
		{
		}

		public IPlugin Create()
		{
			return new L0ModPlugin();
		}
	}

	public class L0ModPlugin : IComparatorPlugin
	{
		private int delta = 20;

		public L0ModPlugin()
		{
			_info = new L0ModInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private void UpdateValue(int n)
		{
			delta = n;
			_block.Invalidate();
		}

		public override void Setup()
		{
			new L0ModSetup(delta, UpdateValue);
		}

		public override void Work()
		{
			ICommImage socket1 = _in[0] as ICommImage;
			ICommImage socket2 = _in[1] as ICommImage;

			IImage[] img1 = socket1.Images;
			IImage[] img2 = socket2.Images;

			_out = new CommSocket(1);

			IResult[] res = new IResult[img2.Length];

			for(int i=0; i<img2.Length; i++)
			{
				double[] data = new double[img1.Length];

				for(int j=0; j<img1.Length; j++)
					data[j] = Compare(img1[j], img2[i]);

				res[i] = new IResult(data);
			}

			_out[0] = new ICommResult(res, 0, socket1.OriginalImages, socket2.OriginalImages,
					socket1.Categories, socket2.Categories);

			_workdone = true;
		}

		private double Compare(IImage img1, IImage img2)
		{
			if(img1.BPP != 1 && img1.BPP != 4)
				throw new PluginException(Catalog.GetString("Image is not greyscale or floating point."));
			if(img1.BPP != img2.BPP)
				throw new PluginException(Catalog.GetString("Images BPP do not match."));
			if(img1.H != img2.H || img1.W != img2.W)
				throw new PluginException(Catalog.GetString("Images dimensions do not match."));

			double sum = 0;

			if(img1.BPP == 1)
				for(int i=0; i<img1.Data.Length; i++)
				{
					int diff = Math.Abs(img1.Data[i] - img2.Data[i]);
					if(diff > delta)
						sum++;
				}
			else
				for(int y=0; y<img1.H; y++)
					for(int x=0; x<img1.W; x++)
					{
						float diff = Math.Abs((float)img1[x, y] - (float)img2[x, y]);
						if(diff > delta)
							sum++;
					}

			return sum / (img1.W * img1.H);
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");

			root.InnerText = delta.ToString();
			
			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			UpdateValue(Int32.Parse(root.InnerText));
		}

		public override int NumIn		{ get { return 2; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			if(n == 0)
				return Catalog.GetString("Base images.");
			else
				return Catalog.GetString("Test images.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Calculated modified L0 metric.");
		}
	}
}
