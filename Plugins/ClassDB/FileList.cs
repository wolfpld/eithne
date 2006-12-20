using System;
using System.Collections;
using System.Reflection;
using System.IO;
using Gtk;
using Mono.Unix;

namespace Eithne
{
	class FileList : TreeView
	{
		private static Gdk.Pixbuf TestIcon = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "image-test-22.png");
		private static Gdk.Pixbuf BaseIcon = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "image-base-22.png");

		private ArrayList cat;
		private IBlock b;

		private bool HasParent(TreeModel model, TreeIter iter)
		{
			TreeIter ti;

			return model.IterParent(out ti, iter);
		}

		private bool HasParent(TreePath path)
		{
			return path.Depth > 1;
		}

		private void RenderCell(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			if(HasParent(model, iter))
				(cell as CellRendererText).Font = "Normal";
			else
				(cell as CellRendererText).Font = "Bold";
		}

		public FileList(ArrayList cat, IBlock b) : base()
		{
			this.cat = cat;
			this.b = b;

			Model = new TreeStore(typeof(string), typeof(Gdk.Pixbuf), typeof(bool), typeof(string), typeof(Img),
					typeof(Gdk.Pixbuf));

			foreach(Category c in cat)
			{
				Gdk.Pixbuf preview = Preview(((Img)c.Files[0]).Name);

				TreeIter iter = (Model as TreeStore).AppendValues(c.Name, null, null, null, null, preview);

				foreach(Img img in c.Files)
				{
					preview = Preview(img.Name);
					(Model as TreeStore).AppendValues(iter, System.IO.Path.GetFileName(img.Name),
							img.IsTest ? TestIcon : BaseIcon, img.IsTest, img.Name, img, preview);
				}
			}

			HeadersVisible = false;
			Selection.Mode = SelectionMode.Multiple;

			CellRendererText cr = new CellRendererText();
			AppendColumn("Name", cr, "text", 0);
			Columns[0].SetCellDataFunc(cr, RenderCell);
			// TODO zrobić generowanie miniaturek na żądanie
			AppendColumn("Preview", new CellRendererPixbuf(), "pixbuf", 5);
			AppendColumn("Type", new CellRendererPixbuf(), "pixbuf", 1);
		}

		public bool Add(string dir)
		{
			string[] files = Directory.GetFiles(dir, "*");

			Category c = new Category(dir);
			TreeIter iter = (Model as TreeStore).AppendValues(dir);

			foreach(string fn in files)
			{
				bool prv = false;

				try
				{
					Gdk.Pixbuf tmp = new Gdk.Pixbuf(fn);
					if(!prv)
					{
						prv = true;
						(Model as TreeStore).SetValue(iter, 5, Preview(tmp));
					}

					Img img = new Img(fn, false);
					(Model as TreeStore).AppendValues(iter, System.IO.Path.GetFileName(fn), BaseIcon, false, fn, img,
							Preview(tmp));
					c.Files.Add(img);
					b.Invalidate();
				}
				catch(GLib.GException e)
				{
					new DialogMessage(Catalog.GetString("Cannot open specified image.\n<i>") + e.Message + "</i>");
				}
			}

			if(c.Files.Count == 0)
			{
				(Model as TreeStore).Remove(ref iter);
				return false;
			}
			else
			{
				cat.Add(c);
				return true;
			}
		}

		public void TryRemove()
		{
			TreePath[] tp = Selection.GetSelectedRows();
			Array.Reverse(tp);

			foreach(TreePath t in tp)
			{
				TreeIter iter;
				(Model as TreeStore).GetIter(out iter, t);

				// FIXME usuwa pierwsze wystąpienie elementu w liście, a nie wybrane
				// FIXME usunięcie pierwszego elementu powinno zmienić miniaturkę wyświetlaną przy nazwie kategorii
				if(HasParent(t))
				{
					TreePath parent = t.Copy();
					parent.Up();

					TreeIter piter;
					(Model as TreeStore).GetIter(out piter, parent);

					string name = (string)Model.GetValue(iter, 3);
					string parentname = (string)Model.GetValue(piter, 0);

					foreach(Category c in cat)
						if(c.Name == parentname)
						{
							foreach(Img img in c.Files)
								if(img.Name == name)
								{
									c.Files.Remove(img);

									if(c.Files.Count == 0)
									{
										(Model as TreeStore).Remove(ref piter);
										cat.Remove(c);
									}
									else
										(Model as TreeStore).Remove(ref iter);

									break;
								}
							break;
						}
				}
				else
				{
					string name = (string)Model.GetValue(iter, 0);

					foreach(Category c in cat)
						if(c.Name == name)
						{
							cat.Remove(c);
							(Model as TreeStore).Remove(ref iter);
							break;
						}
				}
				b.Invalidate();
			}
		}

		protected override void OnRowActivated(TreePath path, TreeViewColumn column)
		{
			if(HasParent(path))
			{
				TreeIter iter;
				(Model as TreeStore).GetIter(out iter, path);

				if(column.Title == "Name" || column.Title == "Preview")
				{
					string s = (string)Model.GetValue(iter, 3);
					new Preview(s);
				}
				else
				{
					if((bool)Model.GetValue(iter, 2))
						SetBase(iter, true);
					else
						SetTest(iter, true);
					b.Invalidate();
				}
			}
		}

		private void SetBase(TreeIter iter, bool changecat)
		{
			(Model as TreeStore).SetValue(iter, 1, BaseIcon);
			(Model as TreeStore).SetValue(iter, 2, false);
			if(changecat)
				((Img)Model.GetValue(iter, 4)).IsTest = false;
		}

		private void SetTest(TreeIter iter, bool changecat)
		{
			(Model as TreeStore).SetValue(iter, 1, TestIcon);
			(Model as TreeStore).SetValue(iter, 2, true);
			if(changecat)
				((Img)Model.GetValue(iter, 4)).IsTest = true;
		}

		public void Update()
		{
			int ci = 0, fi;
			TreeIter iter;
			bool ok = Model.GetIterFirst(out iter);
			if(!ok)
				return;

			do
			{
				TreeIter child;
				Model.IterChildren(out child, iter);

				fi = 0;

				do
				{
					if(((cat[ci] as Category).Files[fi] as Img).IsTest)
						SetTest(child, false);
					else
						SetBase(child, false);
					fi++;
				}
				while(Model.IterNext(ref child));
				ci++;
			}
			while(Model.IterNext(ref iter));
		}

		public int Count
		{
			get { return cat.Count; }
		}

		public int CountImages
		{
			get
			{
				int n = 0;

				foreach(Category c in cat)
					n += c.Files.Count;

				return n;
			}
		}

		private Gdk.Pixbuf Preview(Gdk.Pixbuf img)
		{
			double scale;

			if(img.Width > img.Height)
				scale = img.Width / 24.0;
			else
				scale = img.Height / 24.0;

			return img.ScaleSimple(Scale(img.Width, scale), Scale(img.Height, scale), Gdk.InterpType.Bilinear);
		}

		private Gdk.Pixbuf Preview(string fn)
		{
			return Preview(new Gdk.Pixbuf(fn));
		}

		private int Scale(int s, double scale)
		{
			int val = (int)(s/scale);

			if(val == 0)
				return 1;
			else
				return val;
		}
	}
}
