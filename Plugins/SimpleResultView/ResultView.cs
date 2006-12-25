using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class ResultViewInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Simple Result View"); }
		}

		public override string ShortName
		{
			get { return "SRV"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin displays which images from database were recognized for given test images."); }
		}
	}

	public class ResultViewFactory : IFactory
	{
		IInfo _info = new ResultViewInfo();
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
			return new ResultViewPlugin();
		}
	}

	public class ResultViewPlugin : IOutPlugin
	{
		Gdk.Pixbuf[] itest = null;
		Gdk.Pixbuf[] ibase = null;
		Gdk.Pixbuf[] thumbs = null;
		int[] res = null;
		int[] cat1 = null;
		int[] cat2 = null;
		bool[] match = null;
		bool invert = false;

		public ResultViewPlugin()
		{
			_info = new ResultViewInfo();
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
			new ResultSetup(invert, UpdateValue);
		}

		public override bool HasSetup
		{
			get { return true; }
		}

		public override void DisplayResults()
		{
			if(!_workdone)
				throw new PluginException(Catalog.GetString("Plugin is not ready to display results."));

			new ResultView(ibase, itest, thumbs, res, cat1, cat2, match);
		}

		public override void Work()
		{
			ICommResult r = _in[0] as ICommResult;

			itest = new Gdk.Pixbuf[r.Length];
			thumbs = new Gdk.Pixbuf[r.Length];
			ibase = new Gdk.Pixbuf[r.OriginalBaseImages.Length];

			double scale;

			for(int i=0; i<itest.Length; i++)
			{
				IImage _img = r.OriginalTestImages[i];
				IImage img = new IImage(_img.BPP, _img.W, _img.H, _img.Data, invert);

				if(invert)
					img.Invert();

				if(img.W > img.H)
					scale = img.W / 256.0;
				else
					scale = img.H / 256.0;

				Gdk.Pixbuf tmp = Utility.CreatePixbuf(img);
				itest[i] = tmp.ScaleSimple(Scale(img.W, scale), Scale(img.H, scale), Gdk.InterpType.Bilinear);

				if(img.W > img.H)
					scale = img.W / 64.0;
				else
					scale = img.H / 64.0;

				thumbs[i] = itest[i].ScaleSimple(Scale(img.W, scale), Scale(img.H, scale), Gdk.InterpType.Bilinear);
			}

			for(int i=0; i<ibase.Length; i++)
			{
				IImage _img = r.OriginalBaseImages[i];
				IImage img = new IImage(_img.BPP, _img.W, _img.H, _img.Data, invert);

				if(invert)
					img.Invert();

				if(img.W > img.H)
					scale = img.W / 256.0;
				else
					scale = img.H / 256.0;

				Gdk.Pixbuf tmp = Utility.CreatePixbuf(img);
				ibase[i] = tmp.ScaleSimple(Scale(img.W, scale), Scale(img.H, scale), Gdk.InterpType.Bilinear);
			}

			res = Utility.FindResultsSimple(r);

			cat1 = r.BaseCategories;
			cat2 = r.TestCategories;

			match = r.Match;

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
			return Catalog.GetString("Results to be viewed.");
		}
	}
}
