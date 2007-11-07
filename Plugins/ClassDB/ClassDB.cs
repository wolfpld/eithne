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

		public Plugin.Base Create()
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

					// set all images to base and add them to list
					for(int i=0; i<c.Files.Count; i++)
					{
						((Img)c.Files[i]).IsTest = false;
						tmp.Add(i);
					}
					
					// remove (100-p)% images from list
					int num = (int)(c.Files.Count * (1 - p));

					while(num-- > 0)
						tmp.RemoveAt(r.Next(tmp.Count));

					// set the remaining images to test
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
		private int progress = 0;

		public TaskInfo(IImage[] a_out, ArrayList a_in, int start, int end, int progress)
		{
			this.a_out = a_out;
			this.a_in = a_in;
			this.start = start;
			this.end = end;
			this.progress = progress;
		}

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

				a_out[i] = IImage.Create(buf, Utility.IsBW(buf) ? BPP.Grayscale : BPP.RGB);

				progress++;
			}
		}

		public int Progress
		{
			get { return progress; }
		}
	}

	public class ClassDBPlugin : Plugin.In
	{
		private ArrayList cat = new ArrayList();
		private ArrayList tasks = new ArrayList();
		private int totalImages;
		private Mutex taskListMutex = new Mutex();

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
			taskListMutex.WaitOne();
			tasks.Clear();
			taskListMutex.ReleaseMutex();

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

			totalImages = test_il.Count + base_il.Count;

			if(MultiThreading)
			{
				// test
				TaskInfo ti1 = new TaskInfo(test_imgarray, test_il, 0, test_il.Count/2);
				TaskInfo ti2 = new TaskInfo(test_imgarray, test_il, test_il.Count/2, test_il.Count);

				taskListMutex.WaitOne();
				tasks.Add(ti1);
				tasks.Add(ti2);
				taskListMutex.ReleaseMutex();

				Thread t1 = new Thread(ti1.TaskWork);
				Thread t2 = new Thread(ti2.TaskWork);

				t1.Start();
				t2.Start();

				t1.Join();
				t2.Join();

				int t1progress = ti1.Progress;
				int t2progress = ti2.Progress;

				// base
				ti1 = new TaskInfo(base_imgarray, base_il, 0, base_il.Count/2, t1progress);
				ti2 = new TaskInfo(base_imgarray, base_il, base_il.Count/2, base_il.Count, t2progress);

				taskListMutex.WaitOne();
				tasks.Clear();
				tasks.Add(ti1);
				tasks.Add(ti2);
				taskListMutex.ReleaseMutex();

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
				tasks.Add(t);
				t.TaskWork();
				t = new TaskInfo(base_imgarray, base_il, 0, base_il.Count);
				t.TaskWork();
			}

			_out = new CommSocket(2);
			_out[0] = new ICommImage(base_imgarray, base_imgarray, base_categories);
			_out[1] = new ICommImage(test_imgarray, test_imgarray, test_categories);

			taskListMutex.WaitOne();
			tasks.Clear();
			taskListMutex.ReleaseMutex();

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

		private static string[] matchout  = new string[] { "image/rgb", "image/grayscale" };
		public override string[] MatchOut	{ get { return matchout; } }

		public override float Progress
		{
			get
			{
				int done = 0;

				taskListMutex.WaitOne();
				if(tasks.Count == 0)
				{
					taskListMutex.ReleaseMutex();
					return 1;
				}

				for(int i=0; i<tasks.Count; i++)
				{
					done += ((TaskInfo)tasks[i]).Progress;
				}
				taskListMutex.ReleaseMutex();

				return (float)done/totalImages;
			}
		}
	}
}
