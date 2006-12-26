using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class HaarSetup
	{
		[Widget] Window		HaarWindow;
		[Widget] Button		CloseButton;
		[Widget] SpinButton	HaarSpin;

		public delegate void Callback(int levels);

		public HaarSetup(int levels, Callback c)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Haar.glade", "HaarWindow", null);
			gxml.BindFields(this);

			HaarWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			HaarSpin.Value = levels;
			HaarSpin.ValueChanged += delegate(object o, EventArgs args) { c(HaarSpin.ValueAsInt); };

			HaarWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			HaarWindow.Destroy();
		}
	}
}
