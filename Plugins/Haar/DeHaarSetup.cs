using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class DeHaarSetup
	{
		[Widget] Window		DeHaarWindow;
		[Widget] Button		CloseButton;
		[Widget] SpinButton	HaarSpin;

		public delegate void Callback(int levels);

		public DeHaarSetup(int levels, Callback c)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "DeHaar.glade", "DeHaarWindow", null);
			gxml.BindFields(this);

			DeHaarWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			HaarSpin.Value = levels;
			HaarSpin.ValueChanged += delegate(object o, EventArgs args) { c(HaarSpin.ValueAsInt); };

			DeHaarWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			DeHaarWindow.Destroy();
		}
	}
}
