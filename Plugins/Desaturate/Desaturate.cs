using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class DesaturateInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Desaturate"); }
		}

		public override string ShortName
		{
			get { return "Desat"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin converts color image to greyscale."); }
		}
	}

	public class DesaturateFactory : IFactory
	{
		IInfo _info = new DesaturateInfo();
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
			return new DesaturatePlugin();
		}
	}

	public class DesaturatePlugin : IImgProcPlugin
	{
		public DesaturatePlugin()
		{
			_info = new DesaturateInfo();
		}

		public override void Setup()
		{
		}

		public override bool HasSetup
		{
			get { return false; }
		}

		public override void Work()
		{
			ICommImage socket = _in[0] as ICommImage;
			IImage[] img = socket.Images;
			IImage[] res = new IImage[img.Length];
			_out = new CommSocket(1);

			for(int i=0; i<img.Length; i++)
				res[i] = Desaturate(img[i]);

			_out[0] = new ICommImage(res, socket.OriginalImages, socket.Categories);

			_workdone = true;
		}

		private IImage Desaturate(IImage img)
		{
			if(img.BPP == 1)
				return img;

			if(img.BPP == 4)
				throw new PluginException(Catalog.GetString("Cannot desaturate floating point data"));

			byte[] data = new byte[img.W * img.H];

			for(int i=0; i<img.W * img.H; i++)
			{
				double r = img.Data[i*3];
				double g = img.Data[i*3 + 1];
				double b = img.Data[i*3 + 2];

				data[i] = (byte)(0.3 * r + 0.59 * g + 0.11 * b);
			}

			return new IImage(1, img.W, img.H, data);
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Input image.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Desaturated version of image.");
		}

		private static string[] matchin   = new string[] { "image/rgb", "image/grayscale" };
		private static string[] matchout  = new string[] { "image/grayscale" };

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }
	}
}
