using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class HistogramSetup
	{
		[Widget] Window		HistogramWindow;
		[Widget] SpinButton	Spin;
		[Widget] Button		CloseButton;
		[Widget] CheckButton	BlackButton;
		[Widget] CheckButton	WhiteButton;
		[Widget] CheckButton	SplitHalfButton;

		private int num;
		private Callback c;

		public delegate void Callback(int n, bool black, bool white, bool splithalf);

		public HistogramSetup(int num, bool black, bool white, bool splithalf, Callback c)
		{
			this.c = c;

			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Histogram.glade", "HistogramWindow", null);
			gxml.BindFields(this);

			HistogramWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			this.num = num;
			Spin.Value = num;
			Spin.ValueChanged += SpinChanged;

			BlackButton.Active = black;
			BlackButton.Toggled += delegate(object o, EventArgs args) { c(this.num, BlackButton.Active, WhiteButton.Active, SplitHalfButton.Active); };
			WhiteButton.Active = white;
			WhiteButton.Toggled += delegate(object o, EventArgs args) { c(this.num, BlackButton.Active, WhiteButton.Active, SplitHalfButton.Active); };
			SplitHalfButton.Active = splithalf;
			SplitHalfButton.Toggled += delegate(object o, EventArgs args) { c(this.num, BlackButton.Active, WhiteButton.Active, SplitHalfButton.Active); };

			HistogramWindow.ShowAll();
		}

		private bool IsCorrect(int n)
		{
			return n == 2 || n == 4 || n == 8 || n == 16 || n == 32 || n == 64 || n == 128 || n == 256;
		}

		private void SpinChanged(object o, EventArgs args)
		{
			int val = Spin.ValueAsInt;

			if(!IsCorrect(val))
			{
				if(val > num)
				{
					val = num*2;
					if(val > 256)
						val = 256;
				}
				else if (val < num)
				{
					val = num/2;
					if(val < 2)
						val = 2;
				}

				Spin.Value = val;
			}

			num = val;
			c(val, BlackButton.Active, WhiteButton.Active, SplitHalfButton.Active);
		}

		private void CloseWindow(object o, EventArgs args)
		{
			HistogramWindow.Destroy();
		}
	}
}
