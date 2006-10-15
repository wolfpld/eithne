using System;
using System.Reflection;
using Gtk;
using Glade;

namespace Eithne
{
	public class FFTSetup
	{
		[Widget] Window		FFTWindow;
		[Widget] CheckButton	ZeroButton;
		[Widget] Button		CloseButton;
		[Widget] Label		TitleLabel;

		public delegate void Callback(bool z);

		public FFTSetup(bool z, Callback c, bool dct)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "FFT.glade", "FFTWindow", null);
			gxml.BindFields(this);

			FFTWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			if(dct)
			{
				FFTWindow.Title = "DCT";
				TitleLabel.Text = "<big><big><b>DCT</b></big></big>";
				TitleLabel.UseMarkup = true;
			}

			ZeroButton.Active = z;
			ZeroButton.Toggled += delegate(object o, EventArgs args) { c(ZeroButton.Active); };

			FFTWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			FFTWindow.Destroy();
		}
	}
}
