using System;
using System.Collections;
using Gtk;
using Mono.Unix;

namespace Eithne
{
	class FileList : TreeView
	{
		private ArrayList fl;
		private IBlock b;

		public FileList(ArrayList fl, IBlock b) : base()
		{
			this.fl = fl;
			this.b = b;

			Model = new ListStore(typeof(string), typeof(Gdk.Pixbuf));

			foreach(string s in fl)
				(Model as ListStore).AppendValues(s, Preview(s));

			HeadersVisible = false;
			Selection.Mode = SelectionMode.Multiple;

			AppendColumn("Name", new CellRendererText(), "text", 0);
			AppendColumn("Preview", new CellRendererPixbuf(), "pixbuf", 1);
		}

		public void Add(string fn)
		{
			new Gdk.Pixbuf(fn);

			(Model as ListStore).AppendValues(fn, Preview(fn));
			fl.Add(fn);
			b.Invalidate();
		}

		public void TryRemove()
		{
			TreePath[] tp = Selection.GetSelectedRows();
			Array.Reverse(tp);

			foreach(TreePath t in tp)
			{
				TreeIter iter;
				(Model as ListStore).GetIter(out iter, t);

				// FIXME usuwa pierwsze wystąpienie elementu w liście, a nie wybrane
				fl.Remove(Model.GetValue(iter, 0));
				(Model as ListStore).Remove(ref iter);
				b.Invalidate();
			}
		}

		protected override void OnRowActivated(TreePath path, TreeViewColumn column)
		{
			TreeIter iter;
			(Model as ListStore).GetIter(out iter, path);
			string s = (string)Model.GetValue(iter, 0);
			new Preview(s);
		}

		public int Count
		{
			get { return fl.Count; }
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
