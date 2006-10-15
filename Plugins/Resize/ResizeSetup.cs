using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class ResizeSetup
	{
		[Widget] Window		ResizeSetupWindow;
		[Widget] Button		CloseButton;
		[Widget] RadioButton	AbsoluteButton;
		[Widget] RadioButton	RelativeButton;
		[Widget] HScale		XScale;
		[Widget] HScale		YScale;
		[Widget] RadioButton	NearestButton;
		[Widget] RadioButton	TilesButton;
		[Widget] RadioButton	BilinearButton;
		[Widget] RadioButton	HyperButton;

		public delegate void Callback(bool relative, int x, int y, Gdk.InterpType mode);

		private int rx, ry, ax, ay;
		private Gdk.InterpType mode;
		private Callback c;

		public ResizeSetup(bool relative, int x, int y, Gdk.InterpType mode, Callback c)
		{
			this.mode = mode;
			this.c = c;

			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "ResizeSetup.glade", "ResizeSetupWindow", null);
			gxml.BindFields(this);

			ResizeSetupWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			if(relative)
			{
				RelativeButton.Active = true;
				rx = x;
				ry = y;
				ax = 128;
				ay = 128;
				SetupRelative();
			}
			else
			{
				AbsoluteButton.Active = true;
				ax = x;
				ay = y;
				rx = 50;
				ry = 50;
				SetupAbsolute();
			}

			if(mode == Gdk.InterpType.Nearest)
				NearestButton.Active = true;
			else if(mode == Gdk.InterpType.Tiles)
				TilesButton.Active = true;
			else if(mode == Gdk.InterpType.Bilinear)
				BilinearButton.Active = true;
			else
				HyperButton.Active = true;

			NearestButton.Clicked += delegate(object o, EventArgs args)
				{
					mode = Gdk.InterpType.Nearest;
					UpdateValues();
				};
			TilesButton.Clicked += delegate(object o, EventArgs args)
				{
					mode = Gdk.InterpType.Tiles;
					UpdateValues();
				};
			BilinearButton.Clicked += delegate(object o, EventArgs args)
				{
					mode = Gdk.InterpType.Bilinear;
					UpdateValues();
				};
			HyperButton.Clicked += delegate(object o, EventArgs args)
				{
					mode = Gdk.InterpType.Hyper;
					UpdateValues();
				};

			ResizeSetupWindow.ShowAll();
		}

		private void UpdateValues()
		{
			bool rel = RelativeButton.Active;

			c(rel, rel ? rx : ax, rel ? ry : ay, mode);
		}

		private void SetupRelative()
		{
			XScale.SetRange(1, 200);
			XScale.SetIncrements(1, 10);
			XScale.Value = rx;

			YScale.SetRange(1, 200);
			YScale.SetIncrements(1, 10);
			YScale.Value = ry;
		}

		private void SetupAbsolute()
		{
			XScale.SetRange(1, 1000);
			XScale.SetIncrements(1, 100);
			XScale.Value = ax;

			YScale.SetRange(1, 1000);
			YScale.SetIncrements(1, 100);
			YScale.Value = ay;
		}

		private void CloseWindow(object o, EventArgs args)
		{
			ResizeSetupWindow.Destroy();
		}
	}
}
