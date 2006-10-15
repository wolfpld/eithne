using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class RandomSetup
	{
		[Widget] Window		RandomWindow;
		[Widget] Button		CloseButton;
		[Widget] Button		GenerateButton;
		[Widget] Entry		Seed;
		[Widget] SpinButton	XValue;
		[Widget] SpinButton	YValue;

		private int x, y, seed;
		private Callback c;

		public delegate void Callback(int x, int y, int seed);

		public RandomSetup(int x, int y, int seed, Callback c)
		{
			this.c = c;
			this.x = x;
			this.y = y;
			this.seed = seed;

			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Random.glade", "RandomWindow", null);
			gxml.BindFields(this);

			RandomWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			GenerateButton.Clicked += Generate;

			XValue.Value = x;
			XValue.ValueChanged += SpinChanged;
			YValue.Value = y;
			YValue.ValueChanged += SpinChanged;

			Seed.Text = seed.ToString();
			Seed.Changed += SeedChanged;

			RandomWindow.ShowAll();
		}

		private void SeedChanged(object o, EventArgs args)
		{
			if(Seed.Text == "")
				return;

			try
			{
				seed = Int32.Parse(Seed.Text);
			}
			catch(FormatException e)
			{
				if(Seed.Text.Length == 1)
					Seed.Text = "";
				else
					Seed.Text = seed.ToString();
			}

			c(x, y, seed);
		}

		private void Generate(object o, EventArgs args)
		{
			Random r = new Random();

			Seed.Text = r.Next().ToString();
		}

		private void SpinChanged(object o, EventArgs args)
		{
			x = XValue.ValueAsInt;
			y = YValue.ValueAsInt;

			c(x, y, seed);
		}

		private void CloseWindow(object o, EventArgs args)
		{
			RandomWindow.Destroy();
		}
	}
}
