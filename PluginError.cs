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

		public PluginError(Exception e, Block b, bool setup)
		{
			this.b = b;

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
				TitleText.Text = String.Format(Catalog.GetString("<big><big><b>{0} has encountered error while trying to configure plugin.</b></big></big>"), About.Name);
				BottomText.Text = Catalog.GetString("Please report bug to plugin's author.");
			}
			else
			{
				TitleText.Text = String.Format(Catalog.GetString("<big><big><b>{0} has encountered error while trying to run plugin.</b></big></big>"), About.Name);
			}
			TitleText.UseMarkup = true;

			PluginErrorWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			PluginErrorWindow.Destroy();

			b.ShowError = false;
			MainWindow.RedrawSchematic();
		}
	}
}
