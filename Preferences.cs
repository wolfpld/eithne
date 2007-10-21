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

		// Look and feel
		[Widget] CheckButton	RoundButton;
		[Widget] CheckButton	InnerPathButton;
		[Widget] CheckButton	GradientButton;
		[Widget] CheckButton	SmoothConnectionsButton;
		[Widget] CheckButton	AntialiasingButton;
		[Widget] CheckButton	ProgressButton;
		[Widget] CheckButton	ConnectionAnimationButton;

		[Widget] CheckButton	ChangeBackgroundButton;
		[Widget] Label		RedLabel;
		[Widget] Label		GreenLabel;
		[Widget] Label		BlueLabel;
		[Widget] Label		AlphaLabel;
		[Widget] Scale		RedSlider;
		[Widget] Scale		GreenSlider;
		[Widget] Scale		BlueSlider;
		[Widget] Scale		AlphaSlider;

		// Engine
		[Widget] SpinButton	ThreadSpin;
		[Widget] CheckButton	BlockThreadButton;

		public Preferences()
		{
			Glade.XML gxml = new Glade.XML("Preferences.glade", "PreferencesWindow");
			gxml.BindFields(this);

			PreferencesWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(null, "preferences-desktop-48.png"), new Gdk.Pixbuf(null, "preferences-desktop.png")};

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

			ProgressButton.Active = Config.Get("block/progress", true);
			ProgressButton.Toggled += delegate(object o, EventArgs args) { Config.Set("block/progress", ProgressButton.Active); };

			ConnectionAnimationButton.Active = Config.Get("schematic/connectionanimation", true);
			ConnectionAnimationButton.Toggled += delegate(object o, EventArgs args) { Config.Set("schematic/connectionanimation", ConnectionAnimationButton.Active); };

			ThreadSpin.Value = Config.Get("engine/threads", 2);
			ThreadSpin.ValueChanged += delegate(object o, EventArgs args) { Config.Set("engine/threads", ThreadSpin.ValueAsInt); };

			BlockThreadButton.Active = Config.Get("engine/blockthreads", false);
			BlockThreadButton.Toggled += delegate(object o, EventArgs args) { Config.Set("engine/blockthreads", BlockThreadButton.Active); };

			ChangeBackgroundButton.Active = Config.Get("schematic/changebackground", false);
			SetBackgroundControlsStatus(ChangeBackgroundButton.Active);
			ChangeBackgroundButton.Toggled += delegate(object o, EventArgs args) { Config.Set("schematic/changebackground", ChangeBackgroundButton.Active); SetBackgroundControlsStatus(ChangeBackgroundButton.Active); };

			RedSlider.Value = Config.Get("schematic/red", 128);
			RedSlider.ValueChanged += delegate(object o, EventArgs args) { Config.Set("schematic/red", (int)RedSlider.Value); };

			GreenSlider.Value = Config.Get("schematic/green", 128);
			GreenSlider.ValueChanged += delegate(object o, EventArgs args) { Config.Set("schematic/green", (int)GreenSlider.Value); };

			BlueSlider.Value = Config.Get("schematic/blue", 128);
			BlueSlider.ValueChanged += delegate(object o, EventArgs args) { Config.Set("schematic/blue", (int)BlueSlider.Value); };

			AlphaSlider.Value = Config.Get("schematic/alpha", 255);
			AlphaSlider.ValueChanged += delegate(object o, EventArgs args) { Config.Set("schematic/alpha", (int)AlphaSlider.Value); };


			PreferencesWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			PreferencesWindow.Destroy();
		}

		private void SetBackgroundControlsStatus(bool status)
		{
			RedLabel.Sensitive = status;
			GreenLabel.Sensitive = status;
			BlueLabel.Sensitive = status;

			RedSlider.Sensitive = status;
			GreenSlider.Sensitive = status;
			BlueSlider.Sensitive = status;

			if(MainWindow.HaveAlpha)
			{
				AlphaLabel.Sensitive = status;
				AlphaSlider.Sensitive = status;
			}
			else
			{
				AlphaLabel.Sensitive = false;
				AlphaSlider.Sensitive = false;
			}
		}
	}
}
