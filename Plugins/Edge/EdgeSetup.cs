using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class EdgeSetup
	{
		[Widget] Window		EdgeWindow;
		[Widget] Button		CloseButton;
		[Widget] CheckButton	EnergyButton;

		public delegate void Callback(bool energy);

		public EdgeSetup(bool energy, Callback c)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Edge.glade", "EdgeWindow", null);
			gxml.BindFields(this);

			EdgeWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			EnergyButton.Active = energy;
			EnergyButton.Toggled += delegate(object o, EventArgs args) { c(EnergyButton.Active); };

			EdgeWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			EdgeWindow.Destroy();
		}
	}
}
