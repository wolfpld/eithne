using System;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class PluginAbout
	{
		[Widget] Window		PluginAboutWindow;
		[Widget] Label		Name;
		[Widget] Label		Author;
		[Widget] Label		Desc;
		[Widget] Label		Type;
		[Widget] Button		CloseButton;

		private void CommonTasks(IInfo info, string type)
		{
			Glade.XML gxml = new Glade.XML("PluginAbout.glade", "PluginAboutWindow");
			gxml.BindFields(this);

			PluginAboutWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			PluginAboutWindow.Title = String.Format(Catalog.GetString("About plugin {0}"), info.Name);
			PluginAboutWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(null, "plugin-48.png"), new Gdk.Pixbuf(null, "plugin-16.png")};

			Name.Text = "<big><big><big><b>" + info.Name + " " + info.Version + "</b></big></big></big>";
			Name.UseMarkup = true;
			Author.Text = Catalog.GetString("Author: ") + info.Author;
			Author.UseMarkup = true;
			Desc.Text = "<small>" + info.Description + "</small>";
			Desc.UseMarkup = true;

			Type.Text = "<small>" + type + "</small>";
			Type.UseMarkup = true;

			PluginAboutWindow.ShowAll();
		}

		public PluginAbout(IPlugin p)
		{
			string type;

			if(p is IInPlugin)
				type = Catalog.GetString("Input");
			else if (p is IOutPlugin)
				type = Catalog.GetString("Output");
			else if (p is IImgProcPlugin)
				type = Catalog.GetString("Image processing");
			else if (p is IResProcPlugin)
				type = Catalog.GetString("Result processing");
			else if (p is IComparatorPlugin)
				type = Catalog.GetString("Comparator");
			else
				type = Catalog.GetString("Other");

			CommonTasks(p.Info, type);
		}

		public PluginAbout(IFactory f)
		{
			string type = null;

			switch(f.Type)
			{
				case IType.In:
					type = Catalog.GetString("Input");
					break;

				case IType.Out:
					type = Catalog.GetString("Output");
					break;

				case IType.ImgProc:
					type = Catalog.GetString("Image processing");
					break;

				case IType.ResProc:
					type = Catalog.GetString("Result processing");
					break;

				case IType.Comparator:
					type = Catalog.GetString("Comparator");
					break;

				case IType.Other:
					type = Catalog.GetString("Other");
					break;
			}

			CommonTasks(f.Info, type);
		}

		private void CloseWindow(object o, EventArgs args)
		{
			PluginAboutWindow.Destroy();
		}
	}
}
