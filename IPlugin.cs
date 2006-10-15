using System;
using System.Xml;

namespace Eithne
{
	public class PluginException : Exception
	{
		public PluginException(string msg) : base(msg)
		{}
	}

	public enum IType
	{
		In,
		Out,
		ImgProc,
		ResProc,
		Comparator,
		Other
	}

	public interface IBlock
	{
		void Invalidate();
		void SlotsChanged();
	}

	public abstract class IInfo
	{
		public abstract string Name		{ get; }
		public abstract string ShortName	{ get; }
		public abstract string Version		{ get; }
		public abstract string Author		{ get; }
		public abstract string Description	{ get; }
	}

	public interface IFactory
	{
		IInfo Info	{ get; }
		IType Type	{ get; }

		void Initialize();							// TODO: remove?
		IPlugin Create();
	}

	public abstract class IPlugin
	{
		protected IInfo _info;
		private object _source;
		protected XmlDocument _xmldoc;
		protected bool _workdone = false;
		protected IBlock _block = null;
		protected CommSocket _in = null, _out = null;

		public IInfo Info		{ get { return _info; } }
		public XmlDocument XmlDoc	{ set { _xmldoc = value; } }

		public object Source
		{
			get { return _source; }
			set { _source = value; }
		}

		public virtual XmlNode Config
		{
			get { return null; }
			set {}
		}

		public virtual bool WorkDone
		{
			get { return _workdone; }
			set { _workdone = value; }
		}

		public IBlock Block
		{
			set { _block = value; }
		}

		public abstract void Setup();
		public abstract void Work();

		public virtual bool HasSetup
		{
			get { return true; }
		}

		public abstract int NumIn		{ get; }
		public abstract int NumOut		{ get; }
		
		public CommSocket In
		{
			set { _in = value; }
		}

		public CommSocket Out
		{
			get { return _out; }
		}

		public abstract string DescIn(int n);
		public abstract string DescOut(int n);
	}

	public abstract class IInPlugin : IPlugin
	{
		public override int NumIn			{ get { return 0; } }
		public override string DescIn(int n)		{ return null; }
	}

	public abstract class IOutPlugin : IPlugin
	{
		public override int NumOut			{ get { return 0; } }
		public override string DescOut(int n)		{ return null; }
	}

	public abstract class IImgProcPlugin : IPlugin
	{}

	public abstract class IResProcPlugin : IPlugin
	{}

	public abstract class IComparatorPlugin : IPlugin
	{}

	public abstract class IOtherPlugin : IPlugin
	{
		protected Cairo.Color _color = new Cairo.Color(0.5, 0.5, 0.5, 0.75);

		public Cairo.Color Color
		{
			get { return _color; }
		}
	}

	public class CommSocket
	{
		private ICommObject[] obj;

		public CommSocket(int n)
		{
			obj = new ICommObject[n];
		}

		public ICommObject this [int n]
		{
			get { return obj[n]; }
			set { obj[n] = value; }
		}
	}

	public interface ICommObject
	{}

	public class IImage
	{
		private readonly int w, h;
		private readonly byte[] data;
		private readonly int bpp;

		public IImage(int bpp, int w, int h, byte[] data)
		{
			this.bpp = bpp;
			this.w = w;
			this.h = h;
			this.data = data;
		}

		public int this [int x, int y]
		{
			get { return GetPixel(x, y); }
			set { PutPixel(x, y, value); }
		}

		public int W
		{
			get { return w; }
		}

		public int H
		{
			get { return h; }
		}

		public int BPP
		{
			get { return bpp; }
		}

		public byte[] Data
		{
			get { return data; }
		}

		private int GetPixel(int x, int y)
		{
			if(bpp == 1)
				return data[x + w*y];
			else
				return (data[(x + w*y)*3] << 16) + (data[(x + w*y)*3 + 1] << 8) + data[(x + w*y)*3 + 2];
		}

		private void PutPixel(int x, int y, int val)
		{
			if(bpp == 1)
				data[x + w*y] = (byte)val;
			else
			{
				data[(x + w*y)*3] =	(byte)((val & 0xFF0000) >> 16);
				data[(x + w*y)*3 + 1] =	(byte)((val & 0x00FF00) >> 8);
				data[(x + w*y)*3 + 2] = (byte)(val & 0x0000FF);
			}
		}
	}

	public class ICommImage : ICommObject
	{
		private readonly IImage[] images;
		private readonly IImage[] orig;

		public ICommImage(IImage[] images, IImage[] orig)
		{
			this.images = images;
			this.orig = orig;
		}

		public IImage this [int n]
		{
			get { return images[n]; }
			set { images[n] = value; }
		}

		public int Length
		{
			get { return images.Length; }
		}

		public IImage[] Images
		{
			get { return images; }
		}

		public IImage[] OriginalImages
		{
			get { return orig; }
		}

		public IImage OriginalImage(int n)
		{
			return orig[n];
		}
	}

	public class IResult
	{
		private readonly double[] data;

		public IResult(double[] data)
		{
			this.data = data;
		}

		public double this [int i]
		{
			get { return data[i]; }
		}

		public int Length
		{
			get { return data.Length; }
		}
	}

	public class ICommResult : ICommObject
	{
		private readonly double identity;
		private readonly IImage[] origbase;
		private readonly IImage[] origtest;
		private readonly IResult[] res;

		public ICommResult(IResult[] res, double identity, IImage[] origbase, IImage[] origtest)
		{
			this.identity = identity;
			this.res = res;
			this.origbase = origbase;
			this.origtest = origtest;
		}

		public double this [int itest, int ibase]
		{
			get { return res[itest][ibase]; }
		}

		public IResult this [int n]
		{
			get { return res[n]; }
		}

		public int Length
		{
			get { return res.Length; }
		}

		public double Identity
		{
			get { return identity; }
		}

		public IImage[] OriginalBaseImages
		{
			get { return origbase; }
		}

		public IImage[] OriginalTestImages
		{
			get { return origtest; }
		}

		public double Difference(int itest, int ibase)
		{
			return Math.Abs(identity - this[itest, ibase]);
		}
	}
}
