using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class ResizeWaveletInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Wavelet Resize"); }
		}

		public override string ShortName
		{
			get { return "WResize"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin scales down image using wavelet averages."); }
		}
	}

	public class ResizeWaveletFactory : IFactory
	{
		IInfo _info = new ResizeWaveletInfo();
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
			return new ResizeWaveletPlugin();
		}
	}

	public class ResizeWaveletPlugin : IImgProcPlugin
	{
		private float progress;

		public ResizeWaveletPlugin()
		{
			_info = new ResizeWaveletInfo();
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
			progress = 0;

			ICommImage socket = _in[0] as ICommImage;
			IImage[] img = socket.Images;
			IImage[] ret = new IImage[img.Length];

			for(int i=0; i<img.Length; i++)
			{
				ret[i] = Resize(img[i]);
				progress = (float)i/img.Length;
			}

			_out = new CommSocket(1);
			_out[0] = new ICommImage(ret, socket.OriginalImages, socket.Categories);

			_workdone = true;
		}

		private IImage Resize(IImage img)
		{
			IImage ret = new IImage(BPP.Grayscale, img.W/2, img.H/2);

			for(int y=0; y<img.H/2; y++)
				for(int x=0; x<img.W/2; x++)
				{
					byte tmp1 = (byte)(((byte)img[x*2, y*2] + (byte)img[x*2+1, y*2])/2);
					byte tmp2 = (byte)(((byte)img[x*2, y*2+1] + (byte)img[x*2+1, y*2+1])/2);

					ret[x, y] = (byte)((tmp1 + tmp2)/2);
				}

			return ret;
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Input images.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Resized images.");
		}

		private static string[] matchin   = new string[] { "image/grayscale" };
		private static string[] matchout  = new string[] { "image/grayscale" };

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }

		public override float Progress { get { return progress; } }
	}
}
