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
		[Widget] SpinButton	DeltaSpin;

		public delegate void Callback(int levels, int cutoff);

		public HaarSetup(int levels, int cutoff, Callback c)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Haar.glade", "HaarWindow", null);
			gxml.BindFields(this);

			HaarWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			HaarSpin.Value = levels;
			HaarSpin.ValueChanged += delegate(object o, EventArgs args) { c(HaarSpin.ValueAsInt, DeltaSpin.ValueAsInt); };

			DeltaSpin.Value = cutoff;
			DeltaSpin.ValueChanged += delegate(object o, EventArgs args) { c(HaarSpin.ValueAsInt, DeltaSpin.ValueAsInt); };

			HaarWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			HaarWindow.Destroy();
		}
	}
}
