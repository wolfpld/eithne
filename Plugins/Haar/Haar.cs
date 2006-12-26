using System;
using System.Collections;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class HaarInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Haar Wavelet"); }
		}

		public override string ShortName
		{
			get { return "Haar"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin calculates Haar wavelet."); }
		}
	}

	public class HaarFactory : IFactory
	{
		IInfo _info = new HaarInfo();
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
			return new HaarPlugin();
		}
	}

	public class HaarPlugin : IImgProcPlugin
	{
		private bool energy = false;

		public HaarPlugin()
		{
			_info = new HaarInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private void UpdateValue(bool energy)
		{
			this.energy = energy;

			_block.Invalidate();
		}

		public override void Setup()
		{
			new EdgeSetup(energy, UpdateValue);
		}

		public override void Work()
		{
			ICommImage socket = _in[0] as ICommImage;
			IImage[] img = socket.Images;
			IImage[] ret = new IImage[img.Length];

			for(int i=0; i<img.Length; i++)
				ret[i] = CalcHaar(img[i]);

			_out = new CommSocket(1);
			_out[0] = new ICommImage(ret, socket.OriginalImages, socket.Categories);

			_workdone = true;
		}

		private IImage CalcHaar(IImage img)
		{
			if(img.BPP != 1)
                                throw new PluginException(Catalog.GetString("Image is not in greyscale."));

			return Merge(Haar(img));
		}

		private IImage Merge(ArrayList hw)
		{
			return Merge((IImage)hw[0], (IImage)hw[1], (IImage)hw[2], (IImage)hw[3]);
		}

		private IImage Merge(IImage tl, IImage tr, IImage bl, IImage br)
		{
			int w = ((IImage)tl).W;
			int h = ((IImage)tl).H;

			IImage ret = new IImage(1, w*2, h*2);

			for(int y=0; y<h; y++)
				for(int x=0; x<w; x++)
				{
					ret[x, y] = tl[x, y];
					ret[x + w, y] = tr[x, y];
					ret[x, y + h] = bl[x, y];
					ret[x + w, y + h] = br[x, y];
				}

			return ret;
		}

		private ArrayList Haar(IImage img)
		{
			// transformata po wierszach
			IImage tmp1 = new IImage(1, img.W/2, img.H);
			IImage tmp2 = new IImage(1, img.W/2, img.H);

			for(int y=0; y<img.H; y++)
				for(int x=0; x<img.W/2; x++)
				{
					tmp1[x, y] = (byte)(((byte)img[x*2, y] + (byte)img[x*2+1, y])/2);
					tmp2[x, y] = (byte)(127 + ((byte)img[x*2, y] - (byte)tmp1[x, y]));
				}

			// transformata po kolumnach
			IImage tl = new IImage(1, img.W/2, img.H/2);
			IImage tr = new IImage(1, img.W/2, img.H/2);
			IImage bl = new IImage(1, img.W/2, img.H/2);
			IImage br = new IImage(1, img.W/2, img.H/2);

			for(int y=0; y<img.H/2; y++)
				for(int x=0; x<img.W/2; x++)
				{
					tl[x, y] = (byte)(((byte)tmp1[x, y*2] + (byte)tmp1[x, y*2+1])/2);
					bl[x, y] = (byte)(127 + ((byte)tmp1[x, y*2] - (byte)tl[x, y]));

					tr[x, y] = (byte)(((byte)tmp2[x, y*2] + (byte)tmp2[x, y*2+1])/2);
					br[x, y] = (byte)(127 + ((byte)tmp2[x, y*2] - (byte)tr[x, y]));
				}

			ArrayList ret = new ArrayList();
			ret.Add(tl);
			ret.Add(tr);
			ret.Add(bl);
			ret.Add(br);

			return ret;
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");

			if(energy)
				root.InnerText = "true";
			else
				root.InnerText = "false";

			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			if(root.InnerText == "true")
				UpdateValue(true);
			else
				UpdateValue(false);
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Input image.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Haar wavelet.");
		}
	}
}
