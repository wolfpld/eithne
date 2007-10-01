using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class HistogramInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Histogram"); }
		}

		public override string ShortName
		{
			get { return "Hist"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin calculates histogram of image."); }
		}
	}

	public class HistogramFactory : IFactory
	{
		IInfo _info = new HistogramInfo();
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
			return new HistogramPlugin();
		}
	}

	public class HistogramPlugin : IImgProcPlugin
	{
		private int num = 256;
		private bool black = false;
		private bool white = false;
		private bool splithalf = false;

		public HistogramPlugin()
		{
			_info = new HistogramInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private void UpdateValue(int num, bool black, bool white, bool splithalf)
		{
			this.num = num;
			this.black = black;
			this.white = white;
			this.splithalf = splithalf;

			_block.Invalidate();
		}

		public override void Setup()
		{
			new HistogramSetup(num, black, white, splithalf, UpdateValue);
		}

		public override void Work()
		{
			ICommImage socket = _in[0] as ICommImage;
			IImage[] img = socket.Images;
			IImage[] ret = new IImage[img.Length];

			if(splithalf)
				for(int i=0; i<img.Length; i++)
					ret[i] = SplitHistogram(img[i]);
			else
				for(int i=0; i<img.Length; i++)
					ret[i] = Histogram(img[i]);

			_out = new CommSocket(1);
			_out[0] = new ICommImage(ret, socket.OriginalImages, socket.Categories);

			_workdone = true;
		}

		private IImage SplitHistogram(IImage img)
		{
			int sh = img.H/2;

			IImage img1 = new IImage(BPP.Grayscale, img.W, sh);
			IImage img2 = new IImage(BPP.Grayscale, img.W, img.H-sh);

			for(int y=0; y<sh; y++)
				for(int x=0; x<img.W; x++)
					img1[x, y] = img[x, y];

			for(int y=0; y<img.H-sh; y++)
				for(int x=0; x<img.W; x++)
					img2[x, y] = img[x, y+sh];

			IImage h1 = Histogram(img1);
			IImage h2 = Histogram(img2);

			IImage ret = new IImage(BPP.Grayscale, h1.W+h2.W, 1);

			for(int i=0; i<h1.W; i++)
				ret[i, 0] = h1[i, 0];

			for(int i=0; i<h2.W; i++)
				ret[i+h1.W, 0] = h2[i, 0];

			return ret;
		}

		private IImage Histogram(IImage img)
		{
			int div = BinDivisor(num);
			int max = 0;

			int[] counter = new int[num];
			for(int i=0; i<num; i++)
				counter[i] = 0;

			for(int y=0; y<img.H; y++)
				for(int x=0; x<img.W; x++)
				{
					if(!(black && (byte)img[x, y] == 0) && !(white && (byte)img[x, y] == 255))
					{
						counter[(byte)img[x, y] / div]++;
					
						int tmp = counter[(byte)img[x, y] / div];
					
						if(max < tmp)
							max = tmp;
					}
				}

			IImage res = new IImage(BPP.Grayscale, num, 1);

			for(int i=0; i<num; i++)
				res.Data[i] = (byte)(((double)counter[i] / max) * 255);

			return res;
		}

		private int BinDivisor(int n)
		{
			switch(n)
			{
				case 2:
					return 128;

				case 4:
					return 64;

				case 8:
					return 32;

				case 16:
					return 16;

				case 32:
					return 8;

				case 64:
					return 4;

				case 128:
					return 2;

				case 256:
				default:
					return 1;
			}
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");

			XmlNode n = _xmldoc.CreateNode(XmlNodeType.Element, "num", "");
			n.InnerText = num.ToString();
			root.AppendChild(n);

			n = _xmldoc.CreateNode(XmlNodeType.Element, "black", "");
			if(black)
				n.InnerText = "true";
			else
				n.InnerText = "false";
			root.AppendChild(n);

			n = _xmldoc.CreateNode(XmlNodeType.Element, "white", "");
			if(white)
				n.InnerText = "true";
			else
				n.InnerText = "false";
			root.AppendChild(n);

			n = _xmldoc.CreateNode(XmlNodeType.Element, "splithalf", "");
			if(splithalf)
				n.InnerText = "true";
			else
				n.InnerText = "false";
			root.AppendChild(n);
			
			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			// kompatybilność ze starszymi wersjami wtyczki
			if(root.FirstChild is XmlText)
				UpdateValue(Int32.Parse(root.InnerText), false, false, false);
			else
			{
				int num;
				bool black, white, splithalf;

				XmlNode n = root.SelectSingleNode("num");
				num = Int32.Parse(n.InnerText);

				n = root.SelectSingleNode("black");
				if(n.InnerText == "true")
					black = true;
				else
					black = false;

				n = root.SelectSingleNode("white");
				if(n.InnerText == "true")
					white = true;
				else
					white = false;

				n = root.SelectSingleNode("splithalf");
				if(n.InnerText == "true")
					splithalf = true;
				else
					splithalf = false;

				UpdateValue(num, black, white, splithalf);
			}
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Input image.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Histogram of image.");
		}

		private static string[] matchin   = new string[] { "image/grayscale" };
		private static string[] matchout  = new string[] { "image/grayscale/histogram" };

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }
	}
}
