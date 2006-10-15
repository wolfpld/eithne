using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	class RandomSplit
	{
		[Widget] Window		RandomWindow;
		[Widget] SpinButton	Spin;
		[Widget] Button		SplitButton;
		[Widget] Button		CloseButton;
		[Widget] CheckButton	ExactButton;

		public delegate void Callback(int p, bool exact);

		public RandomSplit(Callback c)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Random.glade", "RandomWindow", null);
			gxml.BindFields(this);

			RandomWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			SplitButton.Clicked += delegate(object o, EventArgs args)
				{
					c(Spin.ValueAsInt, ExactButton.Active);
					RandomWindow.Destroy();
				};

			RandomWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			RandomWindow.Destroy();
		}
	}
}
