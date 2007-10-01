using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class DCTInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Discrete Cosine Transform"); }
		}

		public override string ShortName
		{
			get { return "DCT"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin performs discrete cosine transform."); }
		}
	}

	public class DCTFactory : IFactory
	{
		IInfo _info = new DCTInfo();
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
			return new DCTPlugin();
		}
	}

	public class DCTPlugin : IImgProcPlugin
	{
		private bool zero = true;

		public DCTPlugin()
		{
			_info = new DCTInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private void UpdateValue(bool z)
		{
			zero = z;
			_block.SlotsChanged();
		}

		public override void Setup()
		{
			new FFTSetup(zero, UpdateValue, true);
		}

		public override void Work()
		{
			ICommImage socket = _in[0] as ICommImage;
			IImage[] img = socket.Images;
			IImage[] res = new IImage[img.Length];
			_out = new CommSocket(1);

			for(int i=0; i<img.Length; i++)
				res[i] = DCT(img[i]);

			_out[0] = new ICommImage(res, socket.OriginalImages, socket.Categories);

			FFTW.fftw_cleanup();

			_workdone = true;
		}

		private IImage DCT(IImage img)
		{
			double[] datain = new double[img.W * img.H];
			double[] dataout = new double[img.W * img.H];

			for(int y=0; y<img.H; y++)
				for(int x=0; x<img.W; x++)
					datain[x + y*img.W] = (byte)img[x, y];

			IntPtr plan = FFTW.fftw_plan_r2r_2d(img.H, img.W, datain, dataout, FFTW.Kind.FFTW_REDFT10,
					FFTW.Kind.FFTW_REDFT10, 0);

			FFTW.fftw_execute(plan);

			FFTW.fftw_destroy_plan(plan);

			IImage ret = new IImage(BPP.Float, img.W, img.H);

			for(int y=0; y<img.H; y++)
				for(int x=0; x<img.W; x++)
					ret[x, y] = (float)dataout[x + y*img.W] / 255f;

			if(zero)
				ret[0, 0] = 0f;

			return ret;
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");

			if(zero)
				root.InnerText = "1";
			else
				root.InnerText = "0";
			
			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			if(root.InnerText == "1")
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
			return Catalog.GetString("DCT of image.");
		}

		private static string[] matchin   = new string[] { "image/grayscale" };
		private static string[] matchout  = new string[] { "image/float" };

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }
	}
}
