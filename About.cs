using System;
using Gtk;
using Glade;

namespace Eithne
{
	public class About
	{
		[Widget] Window		AboutWindow;
		[Widget] Image		LogoImage;
		[Widget] Label		ProgramVersion;
		[Widget] Button		CloseButton;

		public static string Version
		{
			get { return "0.2.1"; }
		}

		public About()
		{
			Glade.XML gxml = new Glade.XML("About.glade", "AboutWindow");
			gxml.BindFields(this);

			AboutWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(null, "help-browser-48.png"), new Gdk.Pixbuf(null, "help-browser.png")};

			AboutWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			LogoImage.FromPixbuf = new Gdk.Pixbuf(null, "zsrr.jpg");
			ProgramVersion.Text = String.Format("<big><big><b>Eithne v{0}</b></big></big>", Version);
			ProgramVersion.UseMarkup = true;

			AboutWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			AboutWindow.Destroy();
		}
	}
}
