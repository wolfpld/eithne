using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class CropSetup
	{
		[Widget] Window		CropWindow;
		[Widget] SpinButton	SpinX;
		[Widget] SpinButton	SpinY;
		[Widget] RadioButton	RadioTopLeft;
		[Widget] RadioButton	RadioCenter;
		[Widget] Button		CloseButton;

		private int x, y;
		private bool type;
		private Callback c;

		public delegate void Callback(int x, int y, bool type);

		public CropSetup(int x, int y, bool type, Callback c)
		{
			this.c = c;

			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Crop.glade", "CropWindow", null);
			gxml.BindFields(this);

			CropWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			this.x = x;
			this.y = y;
			this.type = type;

			SpinX.Value = x;
			SpinX.ValueChanged += UpdateValues;

			SpinY.Value = y;
			SpinY.ValueChanged += UpdateValues;

			if(type)
				RadioTopLeft.Active = true;
			else
				RadioCenter.Active = true;

			RadioTopLeft.Clicked += UpdateValues;
			RadioCenter.Clicked += UpdateValues;

			CropWindow.ShowAll();
		}

		private void UpdateValues(object o, EventArgs args)
		{
			type = RadioTopLeft.Active;
			x = SpinX.ValueAsInt;
			y = SpinY.ValueAsInt;

			c(x, y, type);
		}

		private void CloseWindow(object o, EventArgs args)
		{
			CropWindow.Destroy();
		}
	}
}
