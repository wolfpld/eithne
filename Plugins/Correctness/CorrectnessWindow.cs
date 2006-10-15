using System;
using System.Reflection;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	class CategoryList : TreeView
	{
		public CategoryList(Category[] cat) : base()
		{
			Model = new ListStore(typeof(Gdk.Pixbuf), typeof(string));

			foreach(Category c in cat)
			{
				if(c.total != 0)
					(Model as ListStore).AppendValues(c.image, String.Format("{0}/{1} ({2:p})", c.matched, c.total,
											       ((double)c.matched)/c.total));
			}

			Selection.Mode = SelectionMode.None;

			AppendColumn(Catalog.GetString("Category"), new CellRendererPixbuf(), "pixbuf", 0);
			AppendColumn(Catalog.GetString("Correctness"), new CellRendererText(), "text", 1);
		}
	}

	public class Correctness
	{
		[Widget] Window		CorrectnessWindow;
		[Widget] Button		CloseButton;
		[Widget] Label		GeneralResult;
		[Widget] ScrolledWindow	ResultSocket;

		public Correctness(int total, int matched, Category[] cat)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Correctness.glade", "CorrectnessWindow", null);
			gxml.BindFields(this);

			CorrectnessWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			GeneralResult.Text = String.Format(Catalog.GetPluralString(
						"Correctly recognized <b>{0}</b> image out of <b>{1}</b>. ({2:p})",
						"Correctly recognized <b>{0}</b> images out of <b>{1}</b>. ({2:p})", matched),
					matched, total, ((double)matched)/total);
			GeneralResult.UseMarkup = true;

			ResultSocket.AddWithViewport(new CategoryList(cat));

			CorrectnessWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			CorrectnessWindow.Destroy();
		}
	}
}
