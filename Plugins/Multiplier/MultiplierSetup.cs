using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class MultiplierSetup
	{
		[Widget] Window		MultiplierWindow;
		[Widget] SpinButton	Spin;
		[Widget] Button		CloseButton;

		public delegate void Callback(int n);

		public MultiplierSetup(int num, Callback c)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Multiplier.glade", "MultiplierWindow", null);
			gxml.BindFields(this);

			MultiplierWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			Spin.Value = num;
			Spin.ValueChanged += delegate(object o, EventArgs args) { c(Spin.ValueAsInt); };

			MultiplierWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			MultiplierWindow.Destroy();
		}
	}
}
