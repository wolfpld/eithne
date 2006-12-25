using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class FlipInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Flip"); }
		}

		public override string ShortName
		{
			get { return "Flip"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin adds flipped copies of images."); }
		}
	}

	public class FlipFactory : IFactory
	{
		IInfo _info = new FlipInfo();
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
			return new FlipPlugin();
		}
	}

	public class FlipPlugin : IImgProcPlugin
	{
		public FlipPlugin()
		{
			_info = new FlipInfo();
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
			IImage[] ret = new IImage[img.Length * 2];

			int[] cat = new int[img.Length * 2];
			IImage[] origimg = new IImage[img.Length * 2];

			for(int i=0; i<img.Length; i++)
			{
				ret[i*2] = img[i];
				ret[i*2 + 1] = FlipImage(img[i]);

				cat[i*2] = socket.Category(i);
				cat[i*2 + 1] = socket.Category(i);

				origimg[i*2] = socket.OriginalImage(i);
				origimg[i*2 + 1] = socket.OriginalImage(i);
			}

			_out = new CommSocket(1);
			_out[0] = new ICommImage(ret, origimg, cat);

			_workdone = true;
		}

		private IImage FlipImage(IImage img)
		{
			IImage ret = new IImage(img.BPP, img.W, img.H);

			for(int y=0; y<img.H; y++)
				for(int x=0; x<img.W; x++)
					ret[x, y] = img[img.W-x-1, y];

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
			return Catalog.GetString("Input images plus flipped images.");
		}
	}
}
