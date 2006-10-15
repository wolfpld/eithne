using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class XofYSetup
	{
		[Widget] Window		BestWindow;
		[Widget] SpinButton	SpinIn;
		[Widget] SpinButton	SpinMin;
		[Widget] Button		CloseButton;

		private Callback c;

		public delegate void Callback(int x, int y);

		public XofYSetup(int x, int y, Callback c)
		{
			this.c = c;

			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "X-of-Y.glade", "BestWindow", null);
			gxml.BindFields(this);

			BestWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			SpinIn.Value = y;
			SpinIn.ValueChanged += SpinInChanged;

			SetMinValues();

			SpinMin.Value = x;
			SpinMin.ValueChanged += delegate(object o, EventArgs args) { c(SpinMin.ValueAsInt, SpinIn.ValueAsInt); };

			BestWindow.ShowAll();
		}

		private void SetMinValues()
		{
			int vin = SpinIn.ValueAsInt;
			int vmin = SpinMin.ValueAsInt;

			int min = vin/2 + 1;

			SpinMin.SetRange(min, vin);

			if(vmin < min)
				vmin = min;
			else if(vmin > vin)
				vmin = vin;

			SpinMin.Value = vmin;
		}

		private void SpinInChanged(object o, EventArgs args)
		{
			SetMinValues();
			c(SpinMin.ValueAsInt, SpinIn.ValueAsInt);
		}

		private void CloseWindow(object o, EventArgs args)
		{
			BestWindow.Destroy();
		}
	}
}
