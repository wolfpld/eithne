using System;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class About
	{
		[Widget] Window		AboutWindow;
		[Widget] Image		LogoImage;
		[Widget] Label		ProgramVersion;
		[Widget] Button		CloseButton;

		public static string Name
		{
//			get { return "Eithne"; }
			get { return "FaRetSys"; }
		}

		public About()
		{
			Glade.XML gxml = new Glade.XML("About.glade", "AboutWindow");
			gxml.BindFields(this);

			AboutWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(null, "help-browser-48.png"), new Gdk.Pixbuf(null, "help-browser.png")};

			AboutWindow.Title = String.Format(Catalog.GetString("About {0}"), Name);

			AboutWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			LogoImage.FromPixbuf = new Gdk.Pixbuf(null, "zsrr.jpg");
			ProgramVersion.Text = String.Format("<big><big><b>{0} v{1}</b></big></big>", Name, Program.Version);
			ProgramVersion.UseMarkup = true;

			AboutWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			AboutWindow.Destroy();
		}
	}
}
