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
		[Widget] Label		ImageCount;

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

			UpdateCount();

			SimpleDBWindow.ShowAll();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			SimpleDBWindow.Destroy();
		}

		private void UpdateCount()
		{
			if(filelist.Count == 0)
				ImageCount.Text = Catalog.GetString("No images");
			else
				ImageCount.Text = String.Format(Catalog.GetPluralString("{0} image", "{0} images", filelist.Count),
						filelist.Count);
		}

		private void OnAdd(object o, EventArgs args)
		{
			ArrayList errors = new ArrayList();
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

			fs.Response += delegate(object obj, ResponseArgs eargs)
				{
					if(eargs.ResponseId == ResponseType.Accept)
						foreach(string fn in fs.Filenames)
						{
							try
							{
								if(fn.Length > basepath.Length && fn.Substring(0, basepath.Length) == basepath)
									filelist.Add(fn.Substring(basepath.Length));
								else
									filelist.Add(fn);
							}
							catch(GLib.GException)
							{
								errors.Add(fn);
							}
						}
				};
			fs.Run();
			fs.Destroy();

			UpdateCount();

			if(errors.Count != 0)
				new LoadError(errors);
		}

		private void OnRemove(object o, EventArgs args)
		{
			filelist.TryRemove();

			UpdateCount();
		}
	}
}
