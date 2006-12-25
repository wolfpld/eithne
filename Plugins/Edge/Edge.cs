using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class EdgeInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Edge detection"); }
		}

		public override string ShortName
		{
			get { return "Edge"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin detects edges in images."); }
		}
	}

	public class EdgeFactory : IFactory
	{
		IInfo _info = new EdgeInfo();
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
			return new EdgePlugin();
		}
	}

	public class EdgePlugin : IImgProcPlugin
	{
		private bool energy = false;

		public EdgePlugin()
		{
			_info = new EdgeInfo();
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
				ret[i] = DetectEdges(img[i]);

			_out = new CommSocket(1);
			_out[0] = new ICommImage(ret, socket.OriginalImages, socket.Categories);

			_workdone = true;
		}

		private IImage DetectEdges(IImage img)
		{
			if(img.BPP != 1)
				throw new PluginException(Catalog.GetString("Image is not in greyscale."));

			int x = img.W - 1;
			int y = img.H - 1;

			IImage res = new IImage(1, x, y);

			for(int iy=0; iy<y; iy++)
				for(int ix=0; ix<x; ix++)
				{
					int diff = ((byte)img[ix+1, iy] + (byte)img[ix, iy+1] - 2 * (byte)img[ix, iy]) / 2;

					if(energy)
						res[ix, iy] = (byte)Math.Abs(diff);
					else
						res[ix, iy] = (byte)(127 + diff/2);
				}

			return res;
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
			return Catalog.GetString("Detected edges.");
		}
	}
}
