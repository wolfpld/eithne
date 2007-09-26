using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class ImageViewInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Image View"); }
		}

		public override string ShortName
		{
			get { return "ImgView"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin displays images it receives on input."); }
		}
	}

	public class ImageViewFactory : IFactory
	{
		IInfo _info = new ImageViewInfo();
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

		public IPlugin Create()
		{
			return new ImageViewPlugin();
		}
	}

	public class ImageViewPlugin : IOutPlugin
	{
		Gdk.Pixbuf[] images = null;
		Gdk.Pixbuf[] thumbs = null;
		int[] cat = null;
		bool invert = false;

		public ImageViewPlugin()
		{
			_info = new ImageViewInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private void UpdateValue(bool invert)
		{
			this.invert = invert;

			_block.Invalidate();
		}

		public override void Setup()
		{
			new ImageSetup(invert, UpdateValue);
		}

		public override bool HasSetup
		{
			get { return true; }
		}

		public override void DisplayResults()
		{
			if(!_workdone)
				throw new PluginException(Catalog.GetString("Plugin is not ready to display images."));

			new ImageViewWindow(images, thumbs, cat);
		}

		public override void Work()
		{
			ICommImage socket = (ICommImage)_in[0];
			IImage[] img = socket.Images;

			images = new Gdk.Pixbuf[img.Length];
			thumbs = new Gdk.Pixbuf[img.Length];
			cat = socket.Categories;

			double scale;

			for(int i=0; i<img.Length; i++)
			{
				IImage _img = new IImage(img[i].BPP, img[i].W, img[i].H, img[i].Data, invert);
				if(invert)
					_img.Invert();

				images[i] = Utility.CreatePixbuf(_img);

				if(_img.W > _img.H)
					scale = _img.W / 64.0;
				else
					scale = _img.H / 64.0;

				thumbs[i] = images[i].ScaleSimple(Scale(_img.W, scale), Scale(_img.H, scale), Gdk.InterpType.Bilinear);
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

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");

			if(invert)
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

		public override string DescIn(int n)
		{
			return Catalog.GetString("Images to be viewed.");
		}

		private static string[] matchin   = new string[] { "image" };
		public override string[] MatchIn	{ get { return matchin; } }
	}
}
