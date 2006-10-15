using System;
using System.Collections;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class PluginListContent : TextBuffer
	{
		public PluginListContent() : base(null)
		{
			SetupTags();

			TextIter iter = EndIter;

			InsertWithTagsByName(ref iter, "\n", "spacer");
			InsertWithTagsByName(ref iter, Catalog.GetString("Input plugins\n"), "category");
			Append(ref iter, PluginDB.In);
			
			InsertWithTagsByName(ref iter, Catalog.GetString("\nImage processing plugins\n"), "category");
			Append(ref iter, PluginDB.ImgProc);

			InsertWithTagsByName(ref iter, Catalog.GetString("\nComparator plugins\n"), "category");
			Append(ref iter, PluginDB.Comparator);

			InsertWithTagsByName(ref iter, Catalog.GetString("\nResult processing plugins\n"), "category");
			Append(ref iter, PluginDB.ResProc);

			InsertWithTagsByName(ref iter, Catalog.GetString("\nOutput plugins\n"), "category");
			Append(ref iter, PluginDB.Out);

			InsertWithTagsByName(ref iter, Catalog.GetString("\nOther plugins\n"), "category");
			Append(ref iter, PluginDB.Other);
		}

		private void SetupTags()
		{
			TextTag tag = new TextTag("category");
			tag.Weight = Pango.Weight.Bold;
			tag.Scale = 1.5;
			tag.Background = "#ddd";
			tag.BackgroundFullHeight = true;
			TagTable.Add(tag);

			tag = new TextTag("name");
			tag.Weight = Pango.Weight.Bold;
			tag.Scale = 1.3;
			tag.LeftMargin = 15;
			tag.PixelsAboveLines = 5;
			tag.PixelsBelowLines = 5;
			TagTable.Add(tag);

			tag = new TextTag("author");
			tag.LeftMargin = 15;
			tag.PixelsBelowLines = 5;
			TagTable.Add(tag);

			tag = new TextTag("description");
			tag.Style = Pango.Style.Italic;
			tag.LeftMargin = 15;
			TagTable.Add(tag);

			tag = new TextTag("spacer");
			tag.Scale = 0.25;
			TagTable.Add(tag);
		}

		private void Append(ref TextIter iter, ArrayList plugins)
		{
			if(plugins.Count == 0)
			{
				InsertWithTagsByName(ref iter, Catalog.GetString("No plugins available.\n"), "description");
				return;
			}

			foreach(IFactory p in plugins)
				Append(ref iter, p);
		}

		private void Append(ref TextIter iter, IFactory p)
		{
			InsertWithTagsByName(ref iter, p.Info.Name + " " + p.Info.Version + "\n", "name");
			InsertWithTagsByName(ref iter, Catalog.GetString("Author: ") + p.Info.Author + "\n", "author");
			InsertWithTagsByName(ref iter, p.Info.Description + "\n", "description");
		}
	}

	public class PluginList
	{
		[Widget] Window		PluginListWindow;
		[Widget] Image		PluginImage;
		[Widget] TextView	PluginListDisplay;
		[Widget] Button		CloseButton;

		public PluginList()
		{
			Glade.XML gxml = new Glade.XML("PluginList.glade", "PluginListWindow");
			gxml.BindFields(this);

			PluginListWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			PluginListWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(null, "plugin-48.png"),
									new Gdk.Pixbuf(null, "plugin-16.png")};
			PluginImage.FromPixbuf = new Gdk.Pixbuf(null, "plugin-48.png");

			PluginListDisplay.Buffer = new PluginListContent();

			PluginListWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			PluginListWindow.Destroy();
		}
	}
}
