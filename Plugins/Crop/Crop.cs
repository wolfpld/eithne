using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class CropInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Crop"); }
		}

		public override string ShortName
		{
			get { return "Crop"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin crops image."); }
		}
	}

	public class CropFactory : IFactory
	{
		IInfo _info = new CropInfo();
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
			return new CropPlugin();
		}
	}

	public class CropPlugin : IImgProcPlugin
	{
		private int x = 10, y = 10;
		private bool type = true;		// T - top-left, F - center

		public CropPlugin()
		{
			_info = new CropInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private void UpdateValue(int x, int y, bool type)
		{
			this.x = x;
			this.y = y;
			this.type = type;

			_block.Invalidate();
		}

		public override void Setup()
		{
			new CropSetup(x, y, type, UpdateValue);
		}

		public override void Work()
		{
			ICommImage socket = _in[0] as ICommImage;
			IImage[] img = socket.Images;
			IImage[] ret = new IImage[img.Length];

			for(int i=0; i<img.Length; i++)
				ret[i] = Crop(img[i]);

			_out = new CommSocket(1);
			_out[0] = new ICommImage(ret, socket.OriginalImages, socket.Categories);

			_workdone = true;
		}

		private IImage Crop(IImage img)
		{
			IImage ret;

			if(type)		// topleft
			{
				int sx = Math.Min(x, img.W);
				int sy = Math.Min(y, img.H);

				ret = new IImage(img.BPP, sx, sy);

				for(int i=0; i<sx; i++)
					for(int j=0; j<sy; j++)
						ret[i, j] = img[i, j];
			}
			else			// center
			{
				int sx = Math.Min(x, img.W);
				int xoff = (img.W - sx)/2;

				int sy = Math.Min(y, img.H/2);
				int yoff = (img.H+1)/2;

				ret = new IImage(img.BPP, sx, sy);

				for(int i=0; i<sx; i++)
					for(int j=0; j<sy; j++)
						ret[i, j] = img[i+xoff, j+yoff];
			}

			return ret;
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

			n = _xmldoc.CreateNode(XmlNodeType.Element, "type", "");
			if(type)
				n.InnerText = "topleft";
			else
				n.InnerText = "center";
			root.AppendChild(n);

			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			int x, y;
			bool type;

			XmlNode n = root.SelectSingleNode("x");
			x = Int32.Parse(n.InnerText);

			n = root.SelectSingleNode("y");
			y = Int32.Parse(n.InnerText);

			n = root.SelectSingleNode("type");
			if(n.InnerText == "topleft")
				type = true;
			else
				type = false;

			UpdateValue(x, y, type);
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Input image.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Cropped image.");
		}

		private static string[] matchin   = new string[] { "image" };
		private static string[] matchout  = new string[] { "image/rgb", "image/grayscale", "image/float" };

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }
	}
}
