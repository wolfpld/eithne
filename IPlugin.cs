using System;
using System.Xml;
using Mono.Unix;

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

		public virtual IInfo Info		{ get { return _info; } }
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

		public virtual void Invalidate()
		{
			ClearInput();
			ClearOutput();
		}

		protected void ClearInput()
		{
			if(_in != null)
			{
				for(int i=0; i<_in.Length; i++)
					_in[i] = null;
				_in = null;
			}
		}

		protected void ClearOutput()
		{
			if(_out != null)
			{
				for(int i=0; i<_out.Length; i++)
					_out[i] = null;
				_out = null;
			}
		}

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
		public override void Setup()
		{}

		public override bool HasSetup
		{
			get { return false; }
		}

		public abstract void DisplayResults();

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
	{}

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

		public int Length
		{
			get { return obj.Length; }
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
			if(bpp != 1 && bpp != 3 && bpp != 4)
				throw new Exception(Catalog.GetString("BPP must be 1, 3 or 4"));

			this.bpp = bpp;
			this.w = w;
			this.h = h;
			this.data = data;
		}

		public IImage(int bpp, int w, int h)
		{
			if(bpp != 1 && bpp != 3 && bpp != 4)
				throw new Exception(Catalog.GetString("BPP must be 1, 3 or 4"));

			this.bpp = bpp;
			this.w = w;
			this.h = h;

			data = new byte[w * h * bpp];
		}

		// zwracamy object, bo nie wiadomo czy będzie bajt, czy int, czy float, ale wewnątrz wszystko
		// jest jako int traktowane
		public object this [int x, int y]
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

		private unsafe object GetPixel(int x, int y)
		{
			if(bpp == 1)
				return data[x + w*y];
			else if(bpp == 3)
				return (data[(x + w*y)*3] << 16) + (data[(x + w*y)*3 + 1] << 8) + data[(x + w*y)*3 + 2];
			else
				fixed(byte *ptr = data)
				{
					return *(((float*)ptr) + x + w*y);
				}
		}

		private unsafe void PutPixel(int x, int y, object val)
		{
			if(bpp == 1)
				data[x + w*y] = (byte)val;
			else if (bpp == 3)
			{
				data[(x + w*y)*3] =	(byte)(((int)val & 0xFF0000) >> 16);
				data[(x + w*y)*3 + 1] =	(byte)(((int)val & 0x00FF00) >> 8);
				data[(x + w*y)*3 + 2] = (byte)((int)val & 0x0000FF);
			}
			else
				fixed(byte *ptr = data)
				{
					*(((float*)ptr) + x + w*y) = (float)val;
				}
		}
	}

	public class ICommImage : ICommObject
	{
		private readonly IImage[] images;
		private readonly IImage[] orig;
		private readonly int[] categories;

		public ICommImage(IImage[] images, IImage[] orig, int[] categories)
		{
			this.images = images;
			this.orig = orig;
			this.categories = categories;
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

		public int[] Categories
		{
			get { return categories; }
		}

		public int Category(int n)
		{
			return categories[n];
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

		public double[] Data
		{
			get { return data; }
		}
	}

	public class ICommResult : ICommObject
	{
		private readonly double identity;
		private readonly IImage[] origbase;
		private readonly IImage[] origtest;
		private readonly IResult[] res;
		private readonly int[] catbase;
		private readonly int[] cattest;
		private readonly bool[] match;

		public ICommResult(IResult[] res, double identity, IImage[] origbase, IImage[] origtest, int[] catbase, int[] cattest)
		{
			this.identity = identity;
			this.res = res;
			this.origbase = origbase;
			this.origtest = origtest;
			this.catbase = catbase;
			this.cattest = cattest;

			match = new bool[res.Length];

			for(int i=0; i<res.Length; i++)
				match[i] = true;
		}

		public ICommResult(IResult[] res, double identity, IImage[] origbase, IImage[] origtest, int[] catbase, int[] cattest,
				bool[] match)
		{
			this.identity = identity;
			this.res = res;
			this.origbase = origbase;
			this.origtest = origtest;
			this.catbase = catbase;
			this.cattest = cattest;
			this.match = match;
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

		public int[] TestCategories
		{
			get { return cattest; }
		}

		public int[] BaseCategories
		{
			get { return catbase; }
		}

		public bool[] Match
		{
			get { return match; }
		}

		public int TestCategory(int n)
		{
			return cattest[n];
		}

		public int BaseCategory(int n)
		{
			return catbase[n];
		}

		public double Difference(int itest, int ibase)
		{
			return Math.Abs(identity - this[itest, ibase]);
		}
	}
}
