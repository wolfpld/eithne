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

		public override string Version
		{
			get { return "0.2"; }
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

		public ImageViewPlugin()
		{
			_info = new ImageViewInfo();
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
				images[i] = Utility.CreatePixbuf(img[i]);

				if(img[i].W > img[i].H)
					scale = img[i].W / 64.0;
				else
					scale = img[i].H / 64.0;

				thumbs[i] = images[i].ScaleSimple(Scale(img[i].W, scale), Scale(img[i].H, scale), Gdk.InterpType.Bilinear);
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

		public override int NumIn		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Images to be viewed.");
		}
	}
}
