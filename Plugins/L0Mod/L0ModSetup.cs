using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class L0ModSetup
	{
		[Widget] Window		L0ModWindow;
		[Widget] SpinButton	Spin;
		[Widget] Button		CloseButton;

		public delegate void Callback(int n);

		public L0ModSetup(int num, Callback c)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "L0Mod.glade", "L0ModWindow", null);
			gxml.BindFields(this);

			L0ModWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			Spin.Value = num;
			Spin.ValueChanged += delegate(object o, EventArgs args) { c(Spin.ValueAsInt); };

			L0ModWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			L0ModWindow.Destroy();
		}
	}
}
