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

	public class ResultData
	{
		[Widget] Window		ResultDataWindow;
		[Widget] Button		CloseButton;
		[Widget] Table		DataTable;

		public ResultData(Gdk.Pixbuf[] ibase, Gdk.Pixbuf[] itest, IResult[] res, int[] match)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "ResultData.glade", "ResultDataWindow", null);
			gxml.BindFields(this);

			ResultDataWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			DataTable.Resize((uint)ibase.Length + 1, (uint)itest.Length + 1);

			DataTable.Attach(new Image(Assembly.GetExecutingAssembly(), "testbase.png"), 0, 1, 0, 1, 0, 0, 1, 1);

			for(uint i=0; i<ibase.Length; i++)
				DataTable.Attach(new Image(ibase[i]), i+1, i+2, 0, 1, 0, 0, 1, 1);
			for(uint i=0; i<itest.Length; i++)
				DataTable.Attach(new Image(itest[i]), 0, 1, i+1, i+2, 0, 0, 1, 1);

			for(uint b=0; b<ibase.Length; b++)
				for(uint t=0; t<itest.Length; t++)
				{
					Label l = new Label();
					
					if(match[t] == b)
					{
						l.Text = "<b>" + res[t][(int)b].ToString("N") + "</b>";
						l.UseMarkup = true;
					}
					else
						l.Text = res[t][(int)b].ToString("N");

					DataTable.Attach(l, b+1, b+2, t+1, t+2, 0, 0, 1, 1);
				}

			ResultDataWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			ResultDataWindow.Destroy();
		}
	}
}
