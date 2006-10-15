using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class RGBSplitInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("RGB Split"); }
		}

		public override string ShortName
		{
			get { return "RGB"; }
		}

		public override string Version
		{
			get { return "0.1"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin splits image to 3 color subchannels. (Red, Green, Blue)"); }
		}
	}

	public class RGBSplitFactory : IFactory
	{
		IInfo _info = new RGBSplitInfo();
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
			return new RGBSplitPlugin();
		}
	}

	public class RGBSplitPlugin : IImgProcPlugin
	{
		public RGBSplitPlugin()
		{
			_info = new RGBSplitInfo();
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

			IImage[] res1 = new IImage[img.Length];
			IImage[] res2 = new IImage[img.Length];
			IImage[] res3 = new IImage[img.Length];

			for(int i=0; i<img.Length; i++)
			{
				IImage[] rgb = Split(img[i]);

				res1[i] = rgb[0];
				res2[i] = rgb[1];
				res3[i] = rgb[2];
			}

			_out = new CommSocket(3);
			_out[0] = new ICommImage(res1, socket.OriginalImages, socket.Categories);
			_out[1] = new ICommImage(res2, socket.OriginalImages, socket.Categories);
			_out[2] = new ICommImage(res3, socket.OriginalImages, socket.Categories);


			_workdone = true;
		}

		private IImage[] Split(IImage img)
		{
			byte[] r = new byte[img.H * img.W];
			byte[] g = new byte[img.H * img.W];
			byte[] b = new byte[img.H * img.W];

			if(img.BPP == 1)
			{
				for(int y=0; y<img.H; y++)
					for(int x=0; x<img.W; x++)
					{
						byte color = (byte)img[x, y];
						r[x + img.W*y] = color;
						g[x + img.W*y] = color;
						b[x + img.W*y] = color;
					}
			}
			else
			{
				for(int y=0; y<img.H; y++)
					for(int x=0; x<img.W; x++)
					{
						int color = img[x, y];
						r[x + img.W*y] = (byte)((color & 0xFF0000) >> 16);
						g[x + img.W*y] = (byte)((color & 0x00FF00) >> 8);
						b[x + img.W*y] = (byte)(color & 0x0000FF);
					}
			}

			IImage[] ret = new IImage[3];
			ret[0] = new IImage(1, img.W, img.H, r);
			ret[1] = new IImage(1, img.W, img.H, g);
			ret[2] = new IImage(1, img.W, img.H, b);

			return ret;
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return 3; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Input image.");
		}

		public override string DescOut(int n)
		{
			if(n == 0)
				return Catalog.GetString("Red subchannel.");
			else if(n == 1)
				return Catalog.GetString("Green subchannel.");
			else
				return Catalog.GetString("Blue subchannel.");
		}
	}
}
