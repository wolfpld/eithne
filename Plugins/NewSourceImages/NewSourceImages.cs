using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class NewSourceImagesInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("New Source Images"); }
		}

		public override string ShortName
		{
			get { return "SrcImg"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin sets source images of data stream to current images in stream."); }
		}
	}

	public class NewSourceImagesFactory : IFactory
	{
		IInfo _info = new NewSourceImagesInfo();
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

		public Plugin.Base Create()
		{
			return new NewSourceImagesPlugin();
		}
	}

	public class NewSourceImagesPlugin : Plugin.ImgProc
	{
		public NewSourceImagesPlugin()
		{
			_info = new NewSourceImagesInfo();
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
			_out = new CommSocket(1);

			_out[0] = new ICommImage(socket.Images, socket.Images, socket.Categories);

			_workdone = true;
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Input image.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Copy of image with original image changed.");
		}

		private static string[] matchin  = new string[] { "image" };
		private static string[] matchout = new string[] { "image/rgb", "image/grayscale", "image/float" };

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }
	}
}
