﻿using System;
using System.Collections;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class DeHaarInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Inverse Haar Wavelet"); }
		}

		public override string ShortName
		{
			get { return "DeHaar"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin calculates inverse of Haar wavelet."); }
		}
	}

	public class DeHaarFactory : IFactory
	{
		IInfo _info = new DeHaarInfo();
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
			return new DeHaarPlugin();
		}
	}

	public class DeHaarPlugin : Plugin.ImgProc
	{
		private int levels = 3;
		private float progress;

		public DeHaarPlugin()
		{
			_info = new DeHaarInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		private void UpdateValue(int levels)
		{
			this.levels = levels;

			_block.Invalidate();
		}

		public override void Setup()
		{
			new DeHaarSetup(levels, UpdateValue);
		}

		public override void Work()
		{
			progress = 0;

			ICommImage socket = _in[0] as ICommImage;
			IImage[] img = socket.Images;
			IImage[] ret = new IImage[img.Length];

			for(int i=0; i<img.Length; i++)
			{
				ret[i] = CalcHaar(img[i]);
				progress = (float)i/img.Length;
			}

			_out = new CommSocket(1);
			_out[0] = new ICommImage(ret, socket.OriginalImages, socket.Categories);

			_workdone = true;
		}

		private IImage CalcHaar(IImage img)
		{
			return HarrWavelet.Inverse(img, levels - 1);
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");
			root.InnerText = levels.ToString();
			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			levels = Int32.Parse(root.InnerText);

			UpdateValue(levels);
		}

		public override int NumIn		{ get { return 1; } }
		public override int NumOut		{ get { return 1; } }

		public override string DescIn(int n)
		{
			return Catalog.GetString("Haar wavelet.");
		}

		public override string DescOut(int n)
		{
			return Catalog.GetString("Output image.");
		}

		private static string[] matchin   = new string[] { "image/grayscale" };
		private static string[] matchout  = new string[] { "image/grayscale" };

		public override string[] MatchIn	{ get { return matchin; } }
		public override string[] MatchOut	{ get { return matchout; } }

		public override float Progress { get { return progress; } }
	}
}
