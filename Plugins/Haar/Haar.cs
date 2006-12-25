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

			ArrayList hw = Haar(img);

			int w = ((IImage)hw[0]).W;
			int h = ((IImage)hw[0]).H;

			IImage ret = new IImage(1, w*2, h*2);

			for(int y=0; y<w; y++)
				for(int x=0; x<h; x++)
				{
					ret[x, y] = ((IImage)hw[0])[x, y];
					ret[x + w, y] = ((IImage)hw[1])[x, y];
					ret[x, y + h] = ((IImage)hw[2])[x, y];
					ret[x + w, y + h] = ((IImage)hw[3])[x, y];
				}

			return ret;
		}

		private ArrayList Haar(IImage img)
		{
			IImage tl = new IImage(1, img.W/2, img.H/2);
			IImage tr = new IImage(1, img.W/2, img.H/2);
			IImage bl = new IImage(1, img.W/2, img.H/2);
			IImage br = new IImage(1, img.W/2, img.H/2);

			for(int y=0; y<img.H/2; y++)
				for(int x=0; x<img.W/2; x++)
				{
					tl[x, y] = (byte)(((byte)img[x*2, y*2] + (byte)img[x*2+1, y*2] +
							(byte)img[x*2, y*2+1] + (byte)img[x*2+1, y*2+1])/4);
					tr[x, y] = (byte)(127 + ((byte)img[x*2+1, y*2] - (byte)img[x*2, y*2])/2);
					bl[x, y] = (byte)(127 + ((byte)img[x*2, y*2+1] - (byte)img[x*2, y*2])/2);
					br[x, y] = (byte)(127 + ((byte)img[x*2+1, y*2+1] - (byte)img[x*2, y*2])/2);
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
