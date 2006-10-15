using System;
using System.Collections;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	class LoadErrorList : TreeView
	{
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
		}
	}

	class LoadError
	{
		[Widget] Window		LoadErrorWindow;
		[Widget] Image		StopImage;
		[Widget] Button		CloseButton;
		[Widget] ScrolledWindow	ErrorListSocket;

		public LoadError(ArrayList errors)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "LoadError.glade", "LoadErrorWindow", null);
			gxml.BindFields(this);

			LoadErrorWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "dialog-error.png"), new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "dialog-error-16.png")};

			LoadErrorWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			StopImage.FromPixbuf = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "dialog-error.png");

			ErrorListSocket.AddWithViewport(new LoadErrorList(errors));

			LoadErrorWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			LoadErrorWindow.Destroy();
		}
	}
}
