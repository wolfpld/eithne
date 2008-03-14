using System;
using System.Collections;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	class LoadErrorList : TreeView
	{
		private void RenderCell(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			(cell as CellRendererText).Markup = (string)model.GetValue(iter, 0);
		}

		public LoadErrorList(ArrayList errors) : base()
		{
			Model = new ListStore(typeof(string));

			foreach(string e in errors)
			{
				(Model as ListStore).AppendValues(e);
			}

			Selection.Mode = SelectionMode.None;
			HeadersVisible = false;

			CellRendererText cr = new CellRendererText();
			AppendColumn("Image path", cr, "text", 0);
			Columns[0].SetCellDataFunc(cr, new Gtk.TreeCellDataFunc(RenderCell));
		}
	}

	class LoadError
	{
		[Widget] Window		LoadErrorWindow;
		[Widget] Image		StopImage;
		[Widget] Button		CloseButton;
		[Widget] ScrolledWindow	ErrorListSocket;
		[Widget] Label		ErrorText;

		public LoadError(ArrayList errors, string text)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "LoadError.glade", "LoadErrorWindow", null);
			gxml.BindFields(this);

			LoadErrorWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "dialog-error.png"), new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "dialog-error-16.png")};

			LoadErrorWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			StopImage.FromPixbuf = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "dialog-error.png");

			ErrorListSocket.AddWithViewport(new LoadErrorList(errors));

			if(text != null)
			{
				ErrorText.Text = text;
				ErrorText.UseMarkup = true;
			}

			LoadErrorWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			LoadErrorWindow.Destroy();
		}
	}
}
