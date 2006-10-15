using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class RandomInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Random pixels"); }
		}

		public override string ShortName
		{
			get { return "Random"; }
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
			get { return Catalog.GetString("This plugin gets random pixels from image and forms a new one."); }
		}
	}

	public class RandomFactory : IFactory
	{
		IInfo _info = new RandomInfo();
		public IInfo Info
		{
			get { return _info; }
		}

		public IType Type
		{
			get { return IType.ImgProc; }
		}

		public void Initialize()
		{
		}

		public IPlugin Create()
		{
			return new RandomPlugin();
		}
	}

	public class RandomPlugin : IImgProcPlugin
	{
		private int x = 10, y = 10, seed;

		public RandomPlugin()
		{
			Random r = new Random();
			seed = r.Next();
			_info = new RandomInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private void UpdateValue(int x, int y, int seed)
		{
			this.x = x;
			this.y = y;
			this.seed = seed;

			_block.Invalidate();
		}

		public override void Setup()
		{
			new RandomSetup(x, y, seed, UpdateValue);
		}

		public override void Work()
		{
			ICommImage socket = _in[0] as ICommImage;
			IImage[] img = socket.Images;
			IImage[] ret = new IImage[img.Length];

			for(int i=0; i<img.Length; i++)
				ret[i] = RandomPixels(img[i]);

			_out = new CommSocket(1);
			_out[0] = new ICommImage(ret, socket.OriginalImages);

			_workdone = true;
		}

		private IImage RandomPixels(IImage img)
		{
			IImage res = new IImage(img.BPP, x, y, new byte[x*y*img.BPP]);
			Random rand = new Random(seed);

			for(int iy=0; iy<y; iy++)
				for(int ix=0; ix<x; ix++)
				{
					int rx = rand.Next(img.W);
					int ry = rand.Next(img.H);

					res[ix, iy] = img[rx, ry];
				}

			return res;
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");

			XmlNode n = _xmldoc.CreateNode(XmlNodeType.Element, "seed", "");
			n.InnerText = seed.ToString();
			root.AppendChild(n);

			n = _xmldoc.CreateNode(XmlNodeType.Element, "x", "");
			n.InnerText = x.ToString();
			root.AppendChild(n);

			n = _xmldoc.CreateNode(XmlNodeType.Element, "y", "");
			n.InnerText = y.ToString();
			root.AppendChild(n);
			
			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			seed = Int32.Parse(root.SelectSingleNode("seed").InnerText);
			x = Int32.Parse(root.SelectSingleNode("x").InnerText);
			y = Int32.Parse(root.SelectSingleNode("y").InnerText);
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Input image.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Random pixels.");
		}
	}
}
