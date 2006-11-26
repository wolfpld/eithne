using System;
using System.Reflection;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class ResultSetup
	{
		[Widget] Window		ResultSetupWindow;
		[Widget] Button		CloseButton;
		[Widget] CheckButton	InvertButton;

		public delegate void Callback(bool invert);

		public ResultSetup(bool invert, Callback c)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "ResultSetup.glade", "ResultSetupWindow", null);
			gxml.BindFields(this);

			ResultSetupWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			InvertButton.Active = invert;
			InvertButton.Toggled += delegate(object o, EventArgs args) { c(InvertButton.Active); };

			ResultSetupWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			ResultSetupWindow.Destroy();
		}
	}
}
