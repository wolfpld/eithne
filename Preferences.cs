using System;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class Preferences
	{
		[Widget] Window		PreferencesWindow;
		[Widget] Button		CloseButton;
		[Widget] CheckButton	RoundButton;
		[Widget] CheckButton	InnerPathButton;
		[Widget] CheckButton	GradientButton;
		[Widget] CheckButton	SmoothConnectionsButton;
		[Widget] CheckButton	AntialiasingButton;

		public Preferences()
		{
			Glade.XML gxml = new Glade.XML("Preferences.glade", "PreferencesWindow");
			gxml.BindFields(this);

//			PreferencesWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(null, "help-browser-48.png"), new Gdk.Pixbuf(null, "help-browser.png")};

			PreferencesWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			RoundButton.Active = Config.Get("block/round", true);
			RoundButton.Toggled += delegate(object o, EventArgs args) { Config.Set("block/round", RoundButton.Active); };

			InnerPathButton.Active = Config.Get("block/innerpath", true);
			InnerPathButton.Toggled += delegate(object o, EventArgs args) { Config.Set("block/innerpath", InnerPathButton.Active); };

			GradientButton.Active = Config.Get("block/gradient", true);
			GradientButton.Toggled += delegate(object o, EventArgs args) { Config.Set("block/gradient", GradientButton.Active); };

			SmoothConnectionsButton.Active = Config.Get("block/smoothconnections", true);
			SmoothConnectionsButton.Toggled += delegate(object o, EventArgs args) { Config.Set("block/smoothconnections", SmoothConnectionsButton.Active); };

			AntialiasingButton.Active = Config.Get("schematic/antialias", true);
			AntialiasingButton.Toggled += delegate(object o, EventArgs args) { Config.Set("schematic/antialias", AntialiasingButton.Active); };

			PreferencesWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			PreferencesWindow.Destroy();
		}
	}
}
