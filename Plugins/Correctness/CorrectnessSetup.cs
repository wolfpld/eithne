using System;
using System.Reflection;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class CorrectnessSetup
	{
		[Widget] Window		SetupWindow;
		[Widget] Button		CloseButton;
		[Widget] RadioButton	FirstButton;
		[Widget] RadioButton	LastButton;

		public delegate void Callback(bool val);

		public CorrectnessSetup(Callback c, bool first)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Setup.glade", "SetupWindow", null);
			gxml.BindFields(this);

			SetupWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			if(first)
				FirstButton.Active = true;
			else
				LastButton.Active = true;

			FirstButton.Clicked += delegate(object o, EventArgs args) { c(true); };
			LastButton.Clicked += delegate(object o, EventArgs args) { c(false); };

			SetupWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			SetupWindow.Destroy();
		}
	}
}
