using System;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	class FatalError
	{
		[Widget] Window		FatalErrorWindow;
		[Widget] Image		StopImage;
		[Widget] Button		CloseButton;
		[Widget] Label		ErrorText;
		[Widget] Label		LabelUpper;
		[Widget] Label		LabelLower;
		[Widget] TextView	FullErrorText;

		private MainWindow mw;

		public FatalError(Exception e, MainWindow mw)
		{
			this.mw = mw;

			Glade.XML gxml = new Glade.XML("FatalError.glade", "FatalErrorWindow");
			gxml.BindFields(this);

			FatalErrorWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(null, "dialog-error.png"), new Gdk.Pixbuf(null, "dialog-error-16.png")};

			FatalErrorWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			StopImage.FromPixbuf = new Gdk.Pixbuf(null, "dialog-error.png");

			ErrorText.Text = e.Message;
			TextBuffer buf = new TextBuffer(null);
			buf.Text = e.ToString();
			FullErrorText.Buffer = buf;

			LabelUpper.Text = String.Format(Catalog.GetString("<big><big><b>{0} has encountered a fatal error and cannot work correctly anymore.</b></big></big>"), About.Name);
			LabelUpper.UseMarkup = true;
			LabelLower.Text = String.Format(Catalog.GetString("{0} will try to do a rescue save of system schematic to <i>rescue.xml</i>"), About.Name);
			LabelLower.UseMarkup = true;

			FatalErrorWindow.ShowAll();

			Application.Run();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			FatalErrorWindow.Destroy();
			Application.Quit();

			if(mw != null)
				mw.EmergencySave();
		}
	}
}
