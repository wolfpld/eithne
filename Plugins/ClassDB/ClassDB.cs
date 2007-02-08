using System;
using System.Collections;
using System.Threading;
using System.Xml;
using Mono.Unix;

namespace Eithne
{
	public class ClassDBInfo : IInfo
	{
		public override string Name
		{
			get { return Catalog.GetString("Categorised Image Database"); }
		}

		public override string ShortName
		{
			get { return "CatDB"; }
		}

		public override string Author
		{
			get { return "Bartosz Taudul"; }
		}

		public override string Description
		{
			get { return Catalog.GetString("This plugin allows categorization of images and splitting them into base and test images."); }
		}
	}

	public class ClassDBFactory : IFactory
	{
		IInfo _info = new ClassDBInfo();
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
			return new ClassDBPlugin();
		}
	}

	class Img
	{
		private string name;
		private bool istest;

		public Img(string name, bool istest)
		{
			this.name = name;
			this.istest = istest;
		}

		public string Name
		{
			get { return name; }
		}

		public bool IsTest
		{
			get { return istest; }
			set { istest = value; }
		}
	}

	class Category
	{
		private string name;
		private ArrayList fl;

		public Category(string name)
		{
			this.name = name;
			fl = new ArrayList();
		}

		public Category(string name, ArrayList fl)
		{
			this.name = name;
			this.fl = fl;
		}

		public ArrayList Files
		{
			get { return fl; }
		}

		public string Name
		{
			get { return name; }
		}

		public static void Inverse(ArrayList cat)
		{
			foreach(Category c in cat)
				for(int i=0; i<c.Files.Count; i++)
					((Img)c.Files[i]).IsTest = !((Img)c.Files[i]).IsTest;
		}

		public static void ToBase(ArrayList cat)
		{
			foreach(Category c in cat)
				for(int i=0; i<c.Files.Count; i++)
					((Img)c.Files[i]).IsTest = false;
		}

		public static void ToTest(ArrayList cat)
		{
			foreach(Category c in cat)
				for(int i=0; i<c.Files.Count; i++)
					((Img)c.Files[i]).IsTest = true;
		}

		public static void OddEven(ArrayList cat)
		{
			foreach(Category c in cat)
				for(int i=0; i<c.Files.Count; i++)
					if(i%2==0)
						((Img)c.Files[i]).IsTest = false;
					else
						((Img)c.Files[i]).IsTest = true;
		}

		public static void FirstHalf(ArrayList cat)
		{
			foreach(Category c in cat)
			{
				int half = c.Files.Count / 2;

				for(int i=0; i<c.Files.Count; i++)
					if(i < half)
						((Img)c.Files[i]).IsTest = true;
					else
						((Img)c.Files[i]).IsTest = false;
			}
		}

		public static void SecondHalf(ArrayList cat)
		{
			foreach(Category c in cat)
			{
				int half = c.Files.Count / 2;

				for(int i=0; i<c.Files.Count; i++)
					if(i < half)
						((Img)c.Files[i]).IsTest = false;
					else
						((Img)c.Files[i]).IsTest = true;
			}
		}

		public static void RandomSplit(ArrayList cat, double p, bool exact)
		{
			Random r = new Random();

			if(!exact)
				foreach(Category c in cat)
					for(int i=0; i<c.Files.Count; i++)
						if(r.NextDouble() < p)
							((Img)c.Files[i]).IsTest = true;
						else
							((Img)c.Files[i]).IsTest = false;
			else
				foreach(Category c in cat)
				{
					ArrayList tmp = new ArrayList();

					// ustawienie wszystkich obrazów na bazowe i dodanie ich do listy
					for(int i=0; i<c.Files.Count; i++)
					{
						((Img)c.Files[i]).IsTest = false;
						tmp.Add(i);
					}
					
					// usunięcie z listy (100-p)% obrazów
					int num = (int)(c.Files.Count * (1 - p));

					while(num-- > 0)
						tmp.RemoveAt(r.Next(tmp.Count));

					// ustawienie tych które przetrwały na testowe
					foreach(int i in tmp)
						((Img)c.Files[i]).IsTest = true;
				}
		}
	}

	public class TaskInfo
	{
		private IImage[] a_out;
		private ArrayList a_in;
		private int start;
		private int end;

		public TaskInfo(IImage[] a_out, ArrayList a_in, int start, int end)
		{
			this.a_out = a_out;
			this.a_in = a_in;
			this.start = start;
			this.end = end;
		}

		public void TaskWork()
		{
			for(int i=start; i<end; i++)
			{
				Gdk.Pixbuf buf = new Gdk.Pixbuf((string)a_in[i]);

				a_out[i] = Utility.CreateImage(buf, Utility.IsBW(buf) ? 1 : 3);
			}
		}
	}

	public class ClassDBPlugin : IInPlugin
	{
		private ArrayList cat = new ArrayList();

		public ClassDBPlugin()
		{
			_info = new ClassDBInfo();
		}

		public override XmlNode Config
		{
			get { return GetConfig(); }
			set { LoadConfig(value); }
		}

		public override void Setup()
		{
			new ClassDBSetup(cat, _block);
		}


		public override void Work()
		{
			if(cat.Count == 0)
				throw new PluginException(Catalog.GetString("No categories in list"));

			bool MultiThreading = Eithne.Config.Get("engine/blockthreads", false);

			ArrayList test_cl = new ArrayList();
			ArrayList test_il = new ArrayList();

			ArrayList base_cl = new ArrayList();
			ArrayList base_il = new ArrayList();

			for(int i=0; i<cat.Count; i++)
				foreach(Img img in (cat[i] as Category).Files)
				{
					if(img.IsTest)
					{
						test_cl.Add(i);
						test_il.Add(img.Name);
					}
					else
					{
						base_cl.Add(i);
						base_il.Add(img.Name);
					}
				}

			if(test_il.Count == 0)
				throw new PluginException(Catalog.GetString("There are no test images selected"));
			if(base_il.Count == 0)
				throw new PluginException(Catalog.GetString("There are no base images selected"));

			int[] test_categories = (int[])test_cl.ToArray(typeof(int));
			int[] base_categories = (int[])base_cl.ToArray(typeof(int));

			IImage[] test_imgarray = new IImage[test_il.Count];
			IImage[] base_imgarray = new IImage[base_il.Count];

			if(MultiThreading)
			{
				// test
				TaskInfo ti1 = new TaskInfo(test_imgarray, test_il, 0, test_il.Count/2);
				TaskInfo ti2 = new TaskInfo(test_imgarray, test_il, test_il.Count/2, test_il.Count);

				Thread t1 = new Thread(ti1.TaskWork);
				Thread t2 = new Thread(ti2.TaskWork);

				t1.Start();
				t2.Start();

				t1.Join();
				t2.Join();

				// base
				ti1 = new TaskInfo(base_imgarray, base_il, 0, base_il.Count/2);
				ti2 = new TaskInfo(base_imgarray, base_il, base_il.Count/2, base_il.Count);

				t1 = new Thread(ti1.TaskWork);
				t2 = new Thread(ti2.TaskWork);

				t1.Start();
				t2.Start();

				t1.Join();
				t2.Join();
			}
			else
			{
				TaskInfo t = new TaskInfo(test_imgarray, test_il, 0, test_il.Count);
				t.TaskWork();
				t = new TaskInfo(base_imgarray, base_il, 0, base_il.Count);
				t.TaskWork();
			}

			_out = new CommSocket(2);
			_out[0] = new ICommImage(base_imgarray, base_imgarray, base_categories);
			_out[1] = new ICommImage(test_imgarray, test_imgarray, test_categories);

			_workdone = true;
		}

		private XmlNode GetConfig()
		{
			XmlNode root = _xmldoc.CreateNode(XmlNodeType.Element, "config", "");
			
			foreach(Category c in cat)
			{
				XmlNode n = _xmldoc.CreateNode(XmlNodeType.Element, "category", "");
				
				XmlNode n2 = _xmldoc.CreateNode(XmlNodeType.Element, "name", "");
				n2.InnerText = c.Name;
				n.AppendChild(n2);

				foreach(Img img in c.Files)
				{
					n2 = _xmldoc.CreateNode(XmlNodeType.Element, "image", "");
					XmlAttribute attr = _xmldoc.CreateAttribute("test");
					attr.Value = img.IsTest ? "true" : "false";
					n2.Attributes.Append(attr);

					n2.InnerText = img.Name;
					n.AppendChild(n2);
				}

				root.AppendChild(n);
			}

			return root;
		}

		private void LoadConfig(XmlNode root)
		{
			XmlNodeList nl = root.SelectNodes("category");
			ArrayList errors = new ArrayList();

			foreach(XmlNode n in nl)
			{
				XmlNode n2 = n.SelectSingleNode("name");
				Category c = new Category(n2.InnerText);

				XmlNodeList files = n.SelectNodes("image");

				foreach(XmlNode file in files)
				{
					try
					{
						new Gdk.Pixbuf(file.InnerText);
						
						bool test;

						if(file.Attributes["test"].Value == "false")
							test = false;
						else
							test = true;

						c.Files.Add(new Img(file.InnerText, test));
					}
					catch(GLib.GException)
					{
						errors.Add(String.Format(Catalog.GetString("From category <b>{0}</b>: {1}"), n2.InnerText,
								       System.IO.Path.GetFileName(file.InnerText)));
					}
				}

				if(c.Files.Count != 0)
					cat.Add(c);
			}

			if(errors.Count != 0)
				new LoadError(errors, null);
		}

		public override int NumOut		{ get { return 2; } }

		public override string DescOut(int n)
		{
			if(n == 0)
				return Catalog.GetString("Base images.");
			else
				return Catalog.GetString("Test images.");
		}
	}
}
