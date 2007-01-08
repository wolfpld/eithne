using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class MirrorInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Mirror"); }
		}

		public override string ShortName
		{
			get { return "Mirror"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin adds copies of images which have left half mirrored to the right half."); }
		}
	}

	public class MirrorFactory : IFactory
	{
		IInfo _info = new MirrorInfo();
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
			return new MirrorPlugin();
		}
	}

	public class MirrorPlugin : IImgProcPlugin
	{
		public MirrorPlugin()
		{
			_info = new MirrorInfo();
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
				ret[i*2 + 1] = MirrorImage(img[i]);

				cat[i*2] = socket.Category(i);
				cat[i*2 + 1] = socket.Category(i);

				origimg[i*2] = socket.OriginalImage(i);
				origimg[i*2 + 1] = socket.OriginalImage(i);
			}

			_out = new CommSocket(1);
			_out[0] = new ICommImage(ret, origimg, cat);

			_workdone = true;
		}

		private IImage MirrorImage(IImage img)
		{
			int half = img.W/2;
			IImage ret = new IImage(img.BPP, half*2, img.H);

			for(int y=0; y<img.H; y++)
				for(int x=0; x<half; x++)
				{
					ret[x, y] = img[x, y];
					ret[half*2-x-1, y] = img[x, y];
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
			return Catalog.GetString("Input images plus mirror images.");
		}
	}
}
