using Cairo;
using System;
using System.Collections;
using System.IO;
using System.Xml;
using Gtk;
using Mono.Unix;

namespace Eithne
{
	class BlockError
	{
		private readonly string name, version, assembly, _class;

		public string Name	{ get { return name; } }
		public string Version	{ get { return version; } }
		public string Assembly	{ get { return assembly; } }
		public string Class	{ get { return _class; } }

		public BlockError(string name, string version, string assembly, string _class)
		{
			this.name = name;
			this.version = version;
			this.assembly = assembly;
			this._class = _class;
		}

		public static bool operator == (BlockError e1, BlockError e2)
		{
			return e1.name == e2.name && e1.version == e2.version && e1.assembly == e2.assembly && e1._class == e2._class;
		}

		public static bool operator != (BlockError e1, BlockError e2)
		{
			return !(e1==e2);
		}

		public override bool Equals(object o)
		{
			if(!(o is BlockError))
				return false;
			return this == (BlockError)o;
		}

		public override int GetHashCode()
		{
			return name.GetHashCode() ^ Version.GetHashCode() ^ Assembly.GetHashCode() ^ Class.GetHashCode();
		}
	}

	class SaveLoad
	{
		private static XmlNode GenInfo(XmlDocument x, string fn)
		{
			XmlNode n, root = x.CreateNode(XmlNodeType.Element, "info", "");

			n = x.CreateNode(XmlNodeType.Element, "version", "");
			n.InnerText = About.Version;
			root.AppendChild(n);

			n = x.CreateNode(XmlNodeType.Element, "filename", "");
			n.InnerText = fn;
			root.AppendChild(n);

			n = x.CreateNode(XmlNodeType.Element, "time", "");
			n.InnerText = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss zzz");
			root.AppendChild(n);

			return root;
		}

		private static XmlNode GenBlock(XmlDocument x, Block b, Hashtable h)
		{
			XmlNode n, n2, root = x.CreateNode(XmlNodeType.Element, "block", "");

			XmlAttribute attr = x.CreateAttribute("id");
			attr.Value = ((int)h[b]).ToString();
			root.Attributes.Append(attr);

			n = x.CreateNode(XmlNodeType.Element, "info", "");
			
			n2 = x.CreateNode(XmlNodeType.Element, "name", "");
			n2.InnerText = b.Plugin.Info.Name;
			n.AppendChild(n2);

			n2 = x.CreateNode(XmlNodeType.Element, "version", "");
			n2.InnerText = b.Plugin.Info.Version;
			n.AppendChild(n2);

			Source s = (Source)b.Plugin.Source;
			n2 = x.CreateNode(XmlNodeType.Element, "assembly", "");
			n2.InnerText = s.Assembly;
			n.AppendChild(n2);
			n2 = x.CreateNode(XmlNodeType.Element, "class", "");
			n2.InnerText = s.Class;
			n.AppendChild(n2);
			
			root.AppendChild(n);

			n = x.CreateNode(XmlNodeType.Element, "x", "");
			n.InnerText = b.X.ToString();
			root.AppendChild(n);

			n = x.CreateNode(XmlNodeType.Element, "y", "");
			n.InnerText = b.Y.ToString();
			root.AppendChild(n);

			b.Plugin.XmlDoc = x;
			n = b.Plugin.Config;
			if(n == null)
				n = x.CreateNode(XmlNodeType.Element, "config", "");
			else if(n.Name != "config")
			{
				n = x.CreateNode(XmlNodeType.Element, "config", "");
				n2 = x.CreateNode(XmlNodeType.Comment, "", "");
				n2.InnerText = " Plugin returns incorrect configuration data! ";
				n.AppendChild(n2);
			}
			root.AppendChild(n);

			for(int i=0; i<b.Plugin.NumOut; i++)
			{
				n = x.CreateNode(XmlNodeType.Element, "socket", "");

				if(b.SocketOut[i].Other != null)
				{
					n2 = x.CreateNode(XmlNodeType.Element, "connection", "");
					n2.InnerText = ((int)h[b.SocketOut[i].Other.Parent]).ToString();
					n.AppendChild(n2);

					n2 = x.CreateNode(XmlNodeType.Element, "other", "");
					n2.InnerText = b.SocketOut[i].Other.Num.ToString();
					n.AppendChild(n2);
				}

				root.AppendChild(n);
			}

			return root;
		}

		private static XmlNode GenSchematic(XmlDocument x, Schematic s)
		{
			XmlNode root = x.CreateNode(XmlNodeType.Element, "schematic", "");
			ArrayList l = s.Blocks;
			Hashtable h = new Hashtable();
			
			for(int i=0; i<l.Count; i++)
				h.Add(l[i], i);

			foreach(Block b in l)
				root.AppendChild(GenBlock(x, b, h));
			
			return root;
		}

		public static void Save(string fn, Schematic s)
		{
			XmlDocument x = new XmlDocument();
			XmlNode n;

			n = x.CreateNode(XmlNodeType.XmlDeclaration, "", "");
			(n as XmlDeclaration).Encoding = "utf-8";
			x.AppendChild(n);

			n = x.CreateNode(XmlNodeType.Comment, "", "");
			n.InnerText = String.Format(" {0} schematic. Do not hand modify. ", About.Name);
			x.AppendChild(n);

			n = x.CreateNode(XmlNodeType.Element, "eithne", "");
			x.AppendChild(n);

			n.AppendChild(GenInfo(x, fn));
			n.AppendChild(GenSchematic(x, s));

			x.Save(fn);
		}

		public static Block LoadBlock(XmlNode root, Context c, Hashtable h, ArrayList errors, Schematic schematic)
		{
			int x = Int32.Parse(root.SelectSingleNode("x").InnerText);
			int y = Int32.Parse(root.SelectSingleNode("y").InnerText);
			string sassembly = root.SelectSingleNode("info/assembly").InnerText;
			string sclass = root.SelectSingleNode("info/class").InnerText;
			int n = Int32.Parse(root.Attributes["id"].Value);
			XmlNode config = root.SelectSingleNode("config");

			Block b;

			Source s = new Source(sassembly, sclass);
			IFactory f = (IFactory)PluginDB.RevOrigin[s];
			if(f == null)
			{
				string sname = root.SelectSingleNode("info/name").InnerText;
				string sversion = root.SelectSingleNode("info/version").InnerText;

				BlockError e = new BlockError(sname, sversion, sassembly, sclass);
				if(!errors.Contains(e))
					errors.Add(e);

				b = null;
			}
			else
			{
				IPlugin p = f.Create();
				p.Source = s;

				b = new Block(schematic, c, p, x, y);
				b.Plugin.Config = config;

				h.Add(n, b);
			}

			return b;
		}

		public static void MakeConnections(XmlNode root, Hashtable h, Block from)
		{
			XmlNodeList nl = root.SelectNodes("socket");

			int i = 0;
			foreach(XmlNode n in nl)
			{
				XmlNode n2 = n.SelectSingleNode("connection");
				if(n2 != null)
				{
					Block to = (Block)h[Int32.Parse(n2.InnerText)];
					int j = Int32.Parse(n.SelectSingleNode("other").InnerText);

					from.SocketOut[i].Connect(to.SocketIn[j]);
				}

				i++;
			}
		}

		public static ArrayList Load(string fn, Schematic s)
		{
			ArrayList errors = new ArrayList();
			ArrayList blocks = new ArrayList();
			Hashtable h = new Hashtable();

			XmlDocument x = new XmlDocument();
			StreamReader sr = new StreamReader(fn);
			x.Load(sr);
			sr.Close();
			XmlNode root = x.DocumentElement;

			if(root.Name != "eithne")
			{
				new DialogMessage(String.Format(Catalog.GetString("File <i>{0}</i> is not an eithne system schematic!"), fn));

				return null;
			}

			XmlNodeList nl = root.SelectNodes("schematic/block");

			Context c = Gdk.Context.CreateDrawable(s.GdkWindow);

			foreach(XmlNode n in nl)
				blocks.Add(LoadBlock(n, c, h, errors, s));

			((IDisposable) c.Target).Dispose();
			((IDisposable) c).Dispose();

			if(errors.Count == 0)
			{
				int i = 0;
				foreach(XmlNode n in nl)
					MakeConnections(n, h, (Block)blocks[i++]);

				return blocks;
			}
			else
			{
				new LoadError(fn, errors);

				return null;
			}
		}
	}
}
