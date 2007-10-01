using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class ResizeInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Resize Image"); }
		}

		public override string ShortName
		{
			get { return "Resize"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin changes size of images"); }
		}
	}

	public class ResizeFactory : IFactory
	{
		IInfo _info = new ResizeInfo();
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
			return new ResizePlugin();
		}
	}

	public class ResizePlugin : IImgProcPlugin
	{
		private bool relative = false;
		private int x = 128;
		private int y = 128;
		private Gdk.InterpType mode = Gdk.InterpType.Bilinear;

		public ResizePlugin()
		{
			_info = new ResizeInfo();
		}

		public override void Setup()
		{
			new ResizeSetup(relative, x, y, mode, UpdateValues);
		}

		public override void Work()
		{
			ICommImage socket = _in[0] as ICommImage;
			IImage[] i1 = socket.Images;
			IImage[] i2 = new IImage[i1.Length];

			for(int i=0; i<i1.Length; i++)
			{
				Gdk.Pixbuf buf = i1[i].CreatePixbuf();

				Gdk.Pixbuf bufout = buf.ScaleSimple(i1[i].W/2, i1[i].H/2, mode);

				i2[i] = IImage.Create(bufout, i1[i].BPP);
			}

			_out = new CommSocket(1);
			_out[0] = new ICommImage(i2, socket.OriginalImages, socket.Categories);

			_workdone = true;
		}

		private void UpdateValues(bool relative, int x, int y, Gdk.InterpType mode)
		{
			this.relative = relative;
			this.x = x;
			this.y = y;
			this.mode = mode;

			_block.Invalidate();
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return 1; } }
		
		public override string DescIn(int n)
		{
			return Catalog.GetString("Input image.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Scaled image.");
		}

		private static string[] matchin   = new string[] { "image" };
		private static string[] matchout  = new string[] { "image/rgb", "image/grayscale", "image/float" };

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }
	}
}
