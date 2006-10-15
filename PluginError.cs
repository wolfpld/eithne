using System;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	class PluginError
	{
		[Widget] Window		PluginErrorWindow;
		[Widget] Image		StopImage;
		[Widget] Button		CloseButton;
		[Widget] Label		ErrorText;
		[Widget] Label		BottomText;
		[Widget] Label		TitleText;
		[Widget] TextView	FullErrorText;

		private Block b;
		private Schematic s;

		public PluginError(Exception e, Block b, Schematic s, bool setup)
		{
			this.b = b;
			this.s = s;

			Glade.XML gxml = new Glade.XML("PluginError.glade", "PluginErrorWindow");
			gxml.BindFields(this);

			PluginErrorWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(null, "dialog-warning.png"), new Gdk.Pixbuf(null, "dialog-warning-16.png")};

			PluginErrorWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			StopImage.FromPixbuf = new Gdk.Pixbuf(null, "dialog-warning.png");

			ErrorText.Text = e.Message;
			TextBuffer buf = new TextBuffer(null);
			buf.Text = e.ToString();
			FullErrorText.Buffer = buf;

			if(setup)
			{
				TitleText.Text = Catalog.GetString("<big><big><b>Eithne encountered error while trying to configure plugin.</b></big></big>");
				BottomText.Text = Catalog.GetString("Please report bug to plugin's author.");
			}
			TitleText.UseMarkup = true;

			PluginErrorWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			PluginErrorWindow.Destroy();

			b.ShowError = false;
			s.Redraw();
		}
	}
}
