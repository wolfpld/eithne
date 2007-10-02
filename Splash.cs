using System;
using Gtk;
using Glade;

namespace Eithne
{
	public class Splash
	{
		[Widget] Window		SplashWindow;
		[Widget] Image		IconImage;
		[Widget] Label		Status;
		[Widget] ProgressBar	ProgressWidget;

		private void ProcessEvents()
		{
			while(Application.EventsPending())
				Application.RunIteration();
		}

		public Splash()
		{
			Glade.XML gxml = new Glade.XML("Splash.glade", "SplashWindow");
			gxml.BindFields(this);

			SplashWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(null, "dialog-information.png"), new Gdk.Pixbuf(null, "dialog-information-16.png")};
			IconImage.FromPixbuf = new Gdk.Pixbuf(null, "dialog-information.png");

			SplashWindow.ShowAll();
			ProcessEvents();
		}

		public void Close()
		{
			SplashWindow.Destroy();
			ProcessEvents();
		}

		public string Message
		{
			set
			{
			       Status.Text = value;
			       ProcessEvents();
			}
		}

		public float Progress
		{
			set
			{
				ProgressWidget.Fraction = value;
			}
		}
	}
}
