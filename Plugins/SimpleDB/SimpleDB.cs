using System;
using System.Collections;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class SimpleDBInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Simple Image Database"); }
		}

		public override string ShortName
		{
			get { return "SimpleDB"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin allows to pass images to image recognition system."); }
		}
	}

	public class SimpleDBFactory : IFactory
	{
		IInfo _info = new SimpleDBInfo();
		public IInfo Info
		{
			get { return _info; }
		}

		public IType Type
		{
			get { return IType.In; }
		}

		public void Initialize()
		{
		}

		public IPlugin Create()
		{
			return new SimpleDBPlugin();
		}
	}

	public class SimpleDBPlugin : IInPlugin
	{
		private ArrayList _fl = new ArrayList();

		public SimpleDBPlugin()
		{
			_info = new SimpleDBInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		public override void Setup()
		{
			new SimpleDBSetup(_fl, _block);
		}

		public override void Work()
		{
			if(_fl.Count == 0)
				throw new PluginException(Catalog.GetString("No images in list"));

			int i = 0;
			IImage[] imgarray = new IImage[_fl.Count];
			int[] categories = new int[_fl.Count];

			foreach(string fn in _fl)
			{
				Gdk.Pixbuf buf = new Gdk.Pixbuf(fn);

				imgarray[i] = Utility.CreateImage(buf, Utility.IsBW(buf) ? 1 : 3);
				categories[i] = i;

				i++;
			}

			_out = new CommSocket(1);
			_out[0] = new ICommImage(imgarray, imgarray, categories);

			_workdone = true;
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");
			
			foreach(string s in _fl)
			{
				XmlNode n = _xmldoc.CreateNode(XmlNodeType.Element, "image", "");
				n.InnerText = s;
				root.AppendChild(n);
			}

			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			XmlNodeList nl = root.SelectNodes("image");
			ArrayList errors = new ArrayList();

			foreach(XmlNode n in nl)
			{
				try
				{
					new Gdk.Pixbuf(n.InnerText);
					
					_fl.Add(n.InnerText);
				}
				catch(GLib.GException)
				{
					errors.Add(n.InnerText);
				}
			}

			if(errors.Count != 0)
				new LoadError(errors);
		}

		public override int NumOut		{ get { return 1; } }

		public override string DescOut(int n)
		{
			return Catalog.GetString("Images.");
		}
	}
}
