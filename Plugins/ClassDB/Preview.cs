using System;
using System.Reflection;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	class Preview
	{
		[Widget] Window		PreviewWindow;
		[Widget] Label		PreviewLabel;
		[Widget] Image		PreviewImage;
		[Widget] Button		CloseButton;

		public Preview(string fn)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Preview.glade", "PreviewWindow", null);
			gxml.BindFields(this);

			Gdk.Pixbuf buf = new Gdk.Pixbuf(fn);

			PreviewWindow.IconList = new Gdk.Pixbuf[1] { buf };

			PreviewWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			PreviewLabel.Text = Catalog.GetString("Image <i>") + fn + "</i>";
			PreviewLabel.UseMarkup = true;

			PreviewImage.FromPixbuf = buf;

			PreviewWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			PreviewWindow.Destroy();
		}
	}
}
