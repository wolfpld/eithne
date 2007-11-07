using System;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	namespace Plugin
	{
		public abstract class Base
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

			public virtual void Lock()
			{
			}

			public virtual void Unlock()
			{
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

			public abstract string[] MatchIn	{ get; }
			public abstract string[] MatchOut	{ get; }

			public virtual float Progress		{ get { return -1; } }
		}

		public abstract class ImgProc : Base
		{}

		public abstract class ResProc : Base
		{}

		public abstract class Comparator : Base
		{}

		public abstract class Other : Base
		{}
	}
}
