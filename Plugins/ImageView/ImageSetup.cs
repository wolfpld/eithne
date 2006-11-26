using System;
using System.Reflection;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class ImageSetup
	{
		[Widget] Window		ImageSetupWindow;
		[Widget] Button		CloseButton;
		[Widget] CheckButton	InvertButton;

		public delegate void Callback(bool invert);

		public ImageSetup(bool invert, Callback c)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "ImageSetup.glade", "ImageSetupWindow", null);
			gxml.BindFields(this);

			ImageSetupWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			InvertButton.Active = invert;
			InvertButton.Toggled += delegate(object o, EventArgs args) { c(InvertButton.Active); };

			ImageSetupWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			ImageSetupWindow.Destroy();
		}
	}
}
