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

			Model = new ListStore(typeof(string), typeof(object));

			foreach(string s in fl)
				(Model as ListStore).AppendValues(s, null);

			HeadersVisible = false;
			Selection.Mode = SelectionMode.Multiple;

			CellRendererText cr = new CellRendererText();
			AppendColumn("Name", cr, "text", 0);
		}

		public void Add(string fn)
		{
			try
			{
				new Gdk.Pixbuf(fn);

				(Model as ListStore).AppendValues(fn, null);
				fl.Add(fn);
				b.Invalidate();
			}
			catch(GLib.GException e)
			{
				new DialogMessage(Catalog.GetString("Cannot open specified image.\n<i>") + e.Message + "</i>");
			}
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
	}
}
