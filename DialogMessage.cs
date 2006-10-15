using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class DialogMessage
	{
		[Widget] Window		DialogMessageWindow;
		[Widget] Image		DialogImage;
		[Widget] Button		CloseButton;
		[Widget] Label		Message;

		public DialogMessage(string msg)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "DialogMessage.glade", "DialogMessageWindow", null);
			gxml.BindFields(this);

			DialogMessageWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "dialog-error.png"), new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "dialog-error-16.png")};
			DialogMessageWindow.Title = "Error";

			DialogMessageWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			DialogImage.FromPixbuf = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "dialog-error.png");

			Message.Text = msg;
			Message.UseMarkup = true;

			DialogMessageWindow.ShowAll();

			Application.Run();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			DialogMessageWindow.Destroy();
			Application.Quit();
		}
	}
}
