using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class HistogramViewInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Histogram View"); }
		}

		public override string ShortName
		{
			get { return "HistView"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin displays histogram graphs."); }
		}
	}

	public class HistogramViewFactory : IFactory
	{
		IInfo _info = new HistogramViewInfo();
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
			return new HistogramViewPlugin();
		}
	}

	public class HistogramViewPlugin : IOutPlugin
	{
		Gdk.Pixbuf[] images = null;
		Gdk.Pixbuf[] thumbs = null;
		int[] cat = null;

		public HistogramViewPlugin()
		{
			_info = new HistogramViewInfo();
		}

		public override void DisplayResults()
		{
			if(!_workdone)
				throw new PluginException(Catalog.GetString("Plugin is not ready to display images."));

			new HistogramViewWindow(images, thumbs, cat);
		}

		public override void Work()
		{
			ICommImage socket = (ICommImage)_in[0];
			IImage[] img = socket.Images;
			IImage[] orig = socket.OriginalImages;

			images = new Gdk.Pixbuf[img.Length];
			thumbs = new Gdk.Pixbuf[img.Length];
			cat = socket.Categories;

			double scale;

			for(int i=0; i<img.Length; i++)
			{
				IImage _img = new IImage(orig[i].BPP, orig[i].W, orig[i].H, orig[i].Data);

				images[i] = HistogramGraph(img[i]);

				Gdk.Pixbuf pixbuf = Utility.CreatePixbuf(_img);

				if(_img.W > _img.H)
					scale = _img.W / 64.0;
				else
					scale = _img.H / 64.0;

				thumbs[i] = pixbuf.ScaleSimple(Scale(_img.W, scale), Scale(_img.H, scale), Gdk.InterpType.Bilinear);
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

		private Gdk.Pixbuf HistogramGraph(IImage img)
		{
			IImage graph = new IImage(BPP.Grayscale, img.W, 260);

			graph.Clear((byte)255);

			for(int i=0; i<img.W; i++)
			{
				int hval = (byte)img[i, 0];

				for(int j=0; j<hval; j++)
				{
					graph[i, 255-j] = (byte)0;
				}

				for(int j=0; j<3; j++)
				{
					graph[i, 259-j] = (byte)((i*255)/img.W);
				}
			}

			return Utility.CreatePixbuf(graph);
		}

		public override int NumIn		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Histograms to be viewed.");
		}

		private static string[] matchin   = new string[] { "image/grayscale/histogram" };
		public override string[] MatchIn	{ get { return matchin; } }
	}
}
