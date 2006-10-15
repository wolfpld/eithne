using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class BestSetup
	{
		[Widget] Window		BestWindow;
		[Widget] SpinButton	Spin;
		[Widget] Button		CloseButton;

		public delegate void Callback(int n);

		public BestSetup(int num, Callback c)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Best.glade", "BestWindow", null);
			gxml.BindFields(this);

			BestWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			Spin.Value = num;
			Spin.ValueChanged += delegate(object o, EventArgs args) { c(Spin.ValueAsInt); };

			BestWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			BestWindow.Destroy();
		}
	}
}
