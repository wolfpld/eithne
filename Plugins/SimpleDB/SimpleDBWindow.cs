using System;
using System.Collections;
using System.Reflection;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class SimpleDBSetup
	{
		[Widget] Window		SimpleDBWindow;
		[Widget] Button		AddButton;
		[Widget] Button		RemoveButton;
		[Widget] Button		CloseButton;
		[Widget] ScrolledWindow	FileListSocket;

		private FileList	filelist;

		public SimpleDBSetup(ArrayList fl, IBlock b)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "SimpleDB.glade", "SimpleDBWindow", null);
			gxml.BindFields(this);

			SimpleDBWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;
			AddButton.Clicked += OnAdd;
			AddButton.Image = new Image(Assembly.GetEntryAssembly(), "list-add.png");
			RemoveButton.Clicked += OnRemove;
			RemoveButton.Image = new Image(Assembly.GetEntryAssembly(), "list-remove.png");

			filelist = new FileList(fl, b);
			FileListSocket.AddWithViewport(filelist);

			SimpleDBWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			SimpleDBWindow.Destroy();
		}

		private void OnAdd(object o, EventArgs args)
		{
			string basepath = AppDomain.CurrentDomain.BaseDirectory;
			FileChooserDialog fs = new FileChooserDialog(Catalog.GetString("Select image to add..."), SimpleDBWindow,
					FileChooserAction.Open, new object[] {Catalog.GetString("Cancel"), ResponseType.Cancel,
					Catalog.GetString("Add"), ResponseType.Accept});

			FileFilter filter = new FileFilter();
//			filter.Name = "Eithne system schematic";
//			filter.AddPattern("*.xml");
//			fs.AddFilter(filter);
			filter = new FileFilter();
			filter.Name = Catalog.GetString("All files");
			filter.AddPattern("*");
			fs.AddFilter(filter);

			fs.SelectMultiple = true;

			fs.Response += delegate(object o, ResponseArgs args)
				{
					if(args.ResponseId == ResponseType.Accept)
						foreach(string fn in fs.Filenames)
						{
							if(fn.Length > basepath.Length && fn.Substring(0, basepath.Length) == basepath)
								filelist.Add(fn.Substring(basepath.Length));
							else
								filelist.Add(fn);
						}
				};
			fs.Run();
			fs.Destroy();
		}

		private void OnRemove(object o, EventArgs args)
		{
			filelist.TryRemove();
		}
	}
}
