using System;
using System.Reflection;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class ImageButton : Button
	{
		private int n;

		public ImageButton(Gtk.Widget w, int n) : base(w)
		{
			this.n = n;
		}
		
		public int Num
		{
			get { return n; }
		}
	}

	public class HistogramViewWindow
	{
		[Widget] Window		HVWindow;
		[Widget] Button		CloseButton;
		[Widget] Label		CounterText;
		[Widget] Label		ImageCategory;
		[Widget] Image		CurrentImage;
		[Widget] ScrolledWindow	ImageListSocket;

		private Gdk.Pixbuf[] img;
		private int[] cat;

		public HistogramViewWindow(Gdk.Pixbuf[] img, Gdk.Pixbuf[] thumbs, int[] cat)
		{
			this.img = img;
			this.cat = cat;

			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "HistogramView.glade", "HVWindow", null);
			gxml.BindFields(this);

			HVWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			CounterText.Text = String.Format(Catalog.GetPluralString("<i>Viewing {0} image histogram</i>", "<i>Viewing {0} image histograms</i>", img.Length), img.Length);
			CounterText.UseMarkup = true;

			ShowImage(0);

			HButtonBox hb = new HButtonBox();

			hb.Layout = ButtonBoxStyle.Start;
			for(int i=0; i<thumbs.Length; i++)
			{
				ImageButton b = new ImageButton(new Image(thumbs[i]), i);
				b.Clicked += OnClicked;
				hb.PackEnd(b, false, false, 0);
			}

			ImageListSocket.AddWithViewport(hb);

			HVWindow.ShowAll();
		}

		private void ShowImage(int n)
		{
			CurrentImage.FromPixbuf = img[n];
			ImageCategory.Text = String.Format(Catalog.GetString("Image category: {0}"), cat[n]);
		}

		private void OnClicked(object o, EventArgs args)
		{
			ShowImage((o as ImageButton).Num);
		}

		private void CloseWindow(object o, EventArgs args)
		{
			img = null;
			cat = null;
			HVWindow.Destroy();
		}
	}
}
