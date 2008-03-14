using System;
using System.Collections;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	class LoadErrorList : TreeView
	{
		public LoadErrorList(ArrayList errors) : base()
		{
			Model = new ListStore(typeof(string), typeof(string), typeof(string));

			foreach(BlockError e in errors)
			{
				(Model as ListStore).AppendValues(e.Name + " " + e.Version, e.Assembly, e.Class);
			}

			Selection.Mode = SelectionMode.None;

			CellRendererText cr = new CellRendererText();
			AppendColumn(Catalog.GetString("Plugin name"), cr, "text", 0);
			Columns[0].SetCellDataFunc(cr, new Gtk.TreeCellDataFunc(RenderCell));
			cr = new CellRendererText();
			AppendColumn(Catalog.GetString("Source assembly"), cr, "text", 1);
			cr = new CellRendererText();
			AppendColumn(Catalog.GetString("Class"), cr, "text", 2);
		}

		private void RenderCell(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			(cell as CellRendererText).Markup = (string)model.GetValue(iter, 0);
		}

	}

	class LoadError
	{
		[Widget] Window		LoadErrorWindow;
		[Widget] Image		StopImage;
		[Widget] Button		CloseButton;
		[Widget] Label		ErrorText;
		[Widget] ScrolledWindow	ErrorListSocket;

		public LoadError(string fn, ArrayList errors)
		{
			Glade.XML gxml = new Glade.XML("LoadError.glade", "LoadErrorWindow");
			gxml.BindFields(this);

			LoadErrorWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(null, "dialog-error.png"), new Gdk.Pixbuf(null, "dialog-error-16.png")};

			LoadErrorWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			StopImage.FromPixbuf = new Gdk.Pixbuf(null, "dialog-error.png");

			ErrorText.Text = String.Format(Catalog.GetString("File <i>{0}</i> cannot be open because the following plugins are not available:"), fn);
			ErrorText.UseMarkup = true;

			ErrorListSocket.AddWithViewport(new LoadErrorList(errors));

			LoadErrorWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			LoadErrorWindow.Destroy();
		}
	}
}
