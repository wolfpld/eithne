using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Mono.Unix;

namespace Eithne
{
	class Source
	{
		private readonly string a, c;

		public Source(string a, string c)
		{
			this.a = a;
			this.c = c;
		}

		public string Assembly
		{
			get { return a; }
		}

		public string Class
		{
			get { return c; }
		}

		public static bool operator == (Source s1, Source s2)
		{
			return s1.Assembly == s2.Assembly && s1.Class == s2.Class;
		}
		
		public static bool operator != (Source s1, Source s2)
		{
			return !(s1 == s2);
		}

		public override bool Equals(object o)
		{
			if(!(o is Source))
				return false;
			return this == (Source)o;
		}

		public override int GetHashCode()
		{
			return a.GetHashCode() ^ c.GetHashCode();
		}
	}

	class PluginDB
	{
		public static ArrayList In = new ArrayList();
		public static ArrayList Out = new ArrayList();
		public static ArrayList ImgProc = new ArrayList();
		public static ArrayList ResProc = new ArrayList();
		public static ArrayList Comparator = new ArrayList();
		public static ArrayList Other = new ArrayList();

		public static Hashtable Origin = new Hashtable();
		public static Hashtable RevOrigin = new Hashtable();

		public static void LoadPlugins(Splash s)
		{
			string basedir = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Plugins");
			string[] tmp = Directory.GetFiles(basedir, "*.dll");
			string[] p = new string[tmp.Length + 1];

			p[0] = System.Reflection.Assembly.GetExecutingAssembly().Location;
			for(int i=0; i<tmp.Length; i++)
				p[i+1] = tmp[i];

			foreach(string fn in p)
			{
				try
				{
					s.Message = Catalog.GetString("Inspecting ") + fn;
					//Console.WriteLine(Catalog.GetString("Inspecting {0}"), fn);
					Assembly a = Assembly.LoadFrom(fn);

					foreach(Type t in a.GetTypes())
					{
						if(t.GetInterface(typeof(IFactory).FullName) != null && !t.IsAbstract)
						{
							IFactory f = (IFactory)Activator.CreateInstance(t);
							f.Initialize();

							//Console.WriteLine(Catalog.GetString("Found {0} plugin {1}"), f.Type, f.Info.Name);

							switch(f.Type)
							{
								case IType.In:
									In.Add(f);
									break;
								case IType.Out:
									Out.Add(f);
									break;
								case IType.ImgProc:
									ImgProc.Add(f);
									break;
								case IType.ResProc:
									ResProc.Add(f);
									break;
								case IType.Comparator:
									Comparator.Add(f);
									break;
								case IType.Other:
									Other.Add(f);
									break;
							}

							Origin.Add(f, new Source(Path.GetFileName(fn), t.FullName));
							RevOrigin.Add(new Source(Path.GetFileName(fn), t.FullName), f);
						}
					}
				}
				catch(Exception e)
				{
					Console.WriteLine(Catalog.GetString("Error while processing {0}: {1}"), fn, e.Message);
				}
			}
		}
	}
}
