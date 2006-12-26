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

			return HarrWavelet.Merge(HarrWavelet.Transform(img));
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
