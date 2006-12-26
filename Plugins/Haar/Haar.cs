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
		private int levels = 3;
		private int cutoff = 0;

		public HaarPlugin()
		{
			_info = new HaarInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private void UpdateValue(int levels, int cutoff)
		{
			this.levels = levels;
			this.cutoff = cutoff;

			_block.Invalidate();
		}

		public override void Setup()
		{
			new HaarSetup(levels, cutoff, UpdateValue);
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

			return HarrWavelet.Transform(img, levels-1, cutoff);
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");

			XmlNode n = _xmldoc.CreateNode(XmlNodeType.Element, "levels", "");
                        n.InnerText = levels.ToString();
                        root.AppendChild(n);

			n = _xmldoc.CreateNode(XmlNodeType.Element, "cutoff", "");
                        n.InnerText = cutoff.ToString();
                        root.AppendChild(n);

			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			levels = Int32.Parse(root.SelectSingleNode("levels").InnerText);
			cutoff = Int32.Parse(root.SelectSingleNode("cutoff").InnerText);

			UpdateValue(levels, cutoff);
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
